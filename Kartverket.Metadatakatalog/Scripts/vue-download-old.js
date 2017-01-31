function clearAlertMessage() {
    $('#feedback-alert .message').html("");
}

function showAlert(message, colorClass) {
    $('#feedback-alert').attr('class', 'alert alert-dismissible alert-' + colorClass);
    $('#feedback-alert .message').html($('#feedback-alert .message').html() + message + "<br/>");
    $('#feedback-alert').show();
}

$(document).on("click", "#remove-all-items", function () {
    $('#remove-all-items-modal').modal('show')
});


function fixUrl(urlen) {
    urlJson = urlen.replace("%3F", "?");
    return urlJson;
}


function getOrderItemName(uuid) {
    var metadata = JSON.parse(localStorage.getItem(uuid + ".metadata"));
    return metadata.name;
}

function IsGeonorge(distributionUrl) {
    return distributionUrl.indexOf("geonorge.no") > -1;
}

function getJsonData(url) {
    var lastSlash = url.lastIndexOf('/');
    var uuid = url.substring(lastSlash + 1);
    var name = getOrderItemName(uuid);
    var jsonUri = fixUrl(url);
    returnData = "";
    if (jsonUri != "") {
        $.ajax({
            url: jsonUri,
            dataType: 'json',
            async: false,
            error: function (jqXHR, textStatus, errorThrown) {
                showAlert("Vennligst fjern " + name + " fra kurv. Feilmelding: " + errorThrown + "<br/>", "danger");
            },
            success: function (data) {
                if (data !== null) {
                    returnData = data;
                }
                else {
                    showAlert("Data mangler for å kunne lastes ned. Vennligst fjern " + name + " fra kurv. <br/>", "danger");
                }
            }
        });
    }
    return returnData;
}


showLoadingAnimation("Laster inn kurv");

$(window).load(function () {
    hideLoadingAnimation();
});


Vue.component('areaoption', {
    props: ['area'],
    template: '<option v-bind:value="area">{{area.name}}</option>'
});

Vue.component('projectionoption', {
    props: ['projection'],
    template: '<option v-bind:value="projection">{{projection.name}}</option>'
});

Vue.component('formatoption', {
    props: ['format'],
    template: '<option v-bind:value="format">{{format.name}}</option>'
});

Vue.config.debug = true;

var mainVueModel = new Vue({
    el: '#vueContainer',
    data: {
        orderItems: [],
        email: "",
        orderResponse: {},
        emailRequired: false,
        selectedProjections: []
    },
    computed: {
        orderRequests: function () {
            var orderItemsGrouped = this.groupBy(this.orderItems, function (orderItem) {
                return [orderItem.metadata.distributionUrl]
            });
            var orderRequests = [];
            orderItemsGrouped.forEach(function (orderItemGroup) {
                var orderRequest = {
                    "distributionUrl": orderItemGroup[0].metadata.orderDistributionUrl,
                    "order": {
                        "email": this.email,
                        "orderLines": []
                    }
                };
                var orderLines = [];
                orderItemGroup.forEach(function (orderItem) {
                    var orderLine = {
                        "metadataUuid": orderItem.metadata.uuid
                    };
                    if (this.getSelectedAreas(orderItem.codelists.selectedAreas).length) {
                        orderLine.areas = this.getSelectedAreas(orderItem.codelists.selectedAreas);
                    }
                    if (this.getSelectedProjections(orderItem.codelists.selectedProjections).length) {
                        orderLine.projections = this.getSelectedProjections(orderItem.codelists.selectedProjections);
                    }
                    if (this.getSelectedFormats(orderItem.codelists.selectedFormats).length) {
                        orderLine.formats = this.getSelectedFormats(orderItem.codelists.selectedFormats);
                    }
                    if (orderItem.codelists.coordinates !== undefined && orderItem.codelists.coordinates !== "") {
                        orderLine.coordinates = orderItem.codelists.coordinates;
                    }
                    if (orderItem.codelists.coordinatesystem !== undefined && orderItem.codelists.coordinatesystem !== "") {
                        orderLine.coordinatesystem = orderItem.codelists.coordinatesystem;
                    }
                    orderRequest.order.orderLines.push(orderLine);
                }.bind(this));
                orderRequests.push(orderRequest);

            }.bind(this));
            return orderRequests;
        },
    },
    created: function () {
        var defaultUrl = "https://nedlasting.geonorge.no/api/capabilities/";
        var orderItemsJson = (localStorage["orderItems"] != null) ? JSON.parse(localStorage["orderItems"]) : [];
        var orderItems = [];
        if (orderItemsJson != []) {
            $(orderItemsJson).each(function (key, val) {
                var metadata = (localStorage[val + ".metadata"] !== undefined) ? JSON.parse(localStorage[val + ".metadata"]) : "";
                var apiUrl = (metadata.distributionUrl !== undefined) ? metadata.distributionUrl : defaultUrl;
                orderItems[key] = {
                    "metadata": metadata,
                    "capabilities": getJsonData(apiUrl + val),
                    "codelists": {
                        "areas": [],
                        "selectedAreas": [],
                        "projections": [],
                        "selectedProjections": [],
                        "availableProjections": [],
                        "formats": [],
                        "selectedFormats": [],
                        "availableFormats": [],
                        "areaTypes": []
                    },
                    "projectionAndFormatIsRequired": false
                }

                var distributionUrl = (orderItems[key].metadata.distributionUrl !== undefined) ? orderItems[key].metadata.distributionUrl : "";

                if (orderItems[key].capabilities._links) {
                    $(orderItems[key].capabilities._links).each(function (index, link) {
                        if (link.rel == "http://rel.geonorge.no/download/area") {
                            orderItems[key].codelists.areas = getJsonData(link.href);
                        }
                        if (link.rel == "http://rel.geonorge.no/download/projection") {
                            orderItems[key].codelists.projections = getJsonData(link.href);
                        }
                        if (link.rel == "http://rel.geonorge.no/download/format") {
                            orderItems[key].codelists.formats = getJsonData(link.href);
                        }
                        if (link.rel == "http://rel.geonorge.no/download/order") {
                            orderItems[key].metadata.orderDistributionUrl = link.href;
                        }
                        if (link.rel == "http://rel.geonorge.no/download/can-download") {
                            orderItems[key].metadata.canDownloadUrl = link.href;
                        }
                    });
                }
                orderItems[key].capabilities.supportsGridSelection = (orderItems[key].capabilities.mapSelectionLayer !== undefined && orderItems[key].capabilities.mapSelectionLayer !== "") ? true : false;

                if (orderItems[key].codelists.areas) {
                    if (orderItems[key].capabilities.supportsPolygonSelection) {
                        orderItems[key].codelists.areas.push(
                            {
                                "name": "Valgt fra kart",
                                "type": "polygon",
                                "code": "Kart",
                                "formats": orderItems[key].codelists.formats,
                                "projections": orderItems[key].codelists.projections
                            }
                        );
                    }
                    $(orderItems[key].codelists.areas).each(function (index, area) {
                        if (area.type !== undefined) {
                            var inArray = false;
                            orderItems[key].codelists.areaTypes.forEach(function (areaType) {
                                if (area.type == areaType.name) {
                                    inArray = true;
                                    areaType.numberOfItems += 1;
                                }
                            });
                            if (!inArray) {
                                orderItems[key].codelists.areaTypes.push({
                                    name: area.type,
                                    numberOfItems: 1
                                });
                            }
                        }
                    })
                }
            }.bind(this));
        }
        this.orderItems = orderItems;
        this.addAreaOptionGroups(orderItems);

    },
    components: {
        'v-select': window.VueSelect.VueSelect
    },
    methods: {
        sendRequests: function () {
            var responseData = [];
            var responseFailed = false;
            this.orderRequests.forEach(function (orderRequest) {
                if (orderRequest.distributionUrl != "") {
                    $.ajax({
                        url: orderRequest.distributionUrl,
                        type: "POST",
                        dataType: 'json',
                        data: JSON.stringify(orderRequest.order),
                        contentType: "application/json",
                        xhrFields: { withCredentials: IsGeonorge(orderRequest.distributionUrl) },
                        async: false,
                        error: function (jqXHR, textStatus, errorThrown) {
                            showAlert(errorThrown, "danger");
                            responseFailed = true;
                        },
                        success: function (data) {
                            if (data !== null) {
                                responseData.push(
                                    {
                                        "distributionUrl": orderRequest.distributionUrl,
                                        "data": data
                                    });
                            }
                            else {
                                showAlert("Feil", "danger");
                                responseFailed = true;
                            }
                        }
                    }).done(function () {
                        $("[data-toggle='tooltip']").tooltip();
                    })
                }
            });
            if (!responseFailed) {
                mainVueModel.removeAllOrderItems();
            }
            this.orderResponse = responseData;
        },
        changeArea: function (orderItem) {
            availableProjections = [];
            selectedAreaProjectionsCodes = [];
            availableFormats = [];
            selectedAreaFormatsNames = [];

            var autoSelectedProjections = [];

            var orderItemHasCoordinates = false;
            orderItem.codelists.selectedAreas.forEach(function (selectedArea) {
                selectedArea.projections.forEach(function (selectedAreaProjection) {
                    if ($.inArray(selectedAreaProjection.code, selectedAreaProjectionsCodes) == -1) {
                        availableProjections.push(selectedAreaProjection);
                        selectedAreaProjectionsCodes.push(selectedAreaProjection.code);
                        console.log(selectedAreaProjection);
                       
                    }
                });
                
                console.log(selectedArea.projections.length);
                /*
                if (selectedArea.projections.length == 1) {
                    autoSelectedProjections.push(selectedAreaProjection);
                }*/

                selectedArea.formats.forEach(function (selectedAreaFormat) {
                    if ($.inArray(selectedAreaFormat.name, selectedAreaFormatsNames) == -1) {
                        availableFormats.push(selectedAreaFormat);
                        selectedAreaFormatsNames.push(selectedAreaFormat.name);
                    }
                });
                if (selectedArea.type == "polygon") {
                    orderItem.codelists.coordinates = selectedArea.coordinates;
                    orderItem.codelists.coordinatesystem = selectedArea.coordinatesystem;
                    orderItemHasCoordinates = true;
                }
                
            });
            
            if (!orderItemHasCoordinates) {
                delete orderItem.codelists.coordinates;
                delete orderItem.codelists.coordinatesystem;
            }
            orderItem.projectionAndFormatIsRequired = this.projectionAndFormatIsRequired(orderItem);
           // orderItem.codelists.selectedProjections = [];
            orderItem.codelists.availableProjections = availableProjections;
            orderItem.codelists.selectedFormats = [];
            orderItem.codelists.availableFormats = availableFormats;
            this.emailRequired = this.orderHasCoordinates();

            mainVueModel.selectedProjections.push(autoSelectedProjections);
           // orderItem.codelists.selectedProjections.push(autoSelectedProjections);
        },

        notSelected: function (elements) {
            var selected = true;
            $(elements).each(function () {
                if (this.selected == true) selected = false;
            })
            return selected;
        },
        getSelectedAreas: function (areas) {
            var selectedAreas = [];
            if (areas !== undefined && areas !== "") {
                areas.forEach(function (area) {
                    selectedAreas.push({
                        "code": area.code,
                        "name": area.name,
                        "type": area.type
                    });
                });
            }
            return selectedAreas;
        },
        getSelectedProjections: function (projections) {
            var selectedProjections = [];
            if (projections !== undefined && projections !== "") {
                projections.forEach(function (projection) {
                    selectedProjections.push({
                        "code": projection.code,
                        "name": projection.name,
                        "codespace": projection.codespace
                    });
                });
            }
            return selectedProjections;
        },
        getSelectedFormats: function (formats) {
            var selectedFormats = [];
            if (formats !== undefined && formats !== "") {
                formats.forEach(function (format) {
                    if (format.version !== undefined) {
                        selectedFormats.push({
                            "name": format.name,
                            "version": format.version
                        });
                    }
                    else {
                        selectedFormats.push({
                            "name": format.name
                        });
                    }

                });
            }
            return selectedFormats;
        },
        groupBy: function (array, groupByFunction) {
            var groups = {};
            array.forEach(function (item) {
                var group = JSON.stringify(groupByFunction(item));
                groups[group] = groups[group] || [];
                groups[group].push(item);
            });
            return Object.keys(groups).map(function (group) {
                return groups[group];
            })
        },
        groupByDistributionUrl: function (array) {
            return this.groupBy(array, function (item) {
                return [item.metadata.distributionUrl];
            })
        },
        resetProjectionSelections: function (orderItem) {
            orderItem.codelists.projections.forEach(function (projection) {
                projection.selected = false;
            });
        },
        resetFormatSelections: function (orderItem) {
            orderItem.codelists.formats.forEach(function (format) {
                format.selected = false;
            });
            $("select[uuid=" + orderItem.metadata.uuid + "].projection-list").val(null);
            $("select[uuid=" + orderItem.metadata.uuid + "].format-list").val(null);
        },
        removeFromLocalStorage: function (uuid) {
            var uuidLength = uuid.length;
            var orderItems = JSON.parse(localStorage["orderItems"]);
            orderItems = $.grep(orderItems, function (value) {
                return value != uuid;
            });
            localStorage["orderItems"] = JSON.stringify(orderItems);
            Object.keys(localStorage)
                    .forEach(function (key) {
                        if (key.substring(0, uuidLength) == uuid) {
                            localStorage.removeItem(key);
                        }
                    });
            updateShoppingCart();
            updateShoppingCartCookie();
        },
        removeOrderItem: function (item) {
            var uuid = item.metadata.uuid;
            this.orderItems = this.orderItems.filter(function (obj) {
                return obj.metadata.uuid !== uuid;
            });
            this.removeFromLocalStorage(uuid);
        },
        removeAllOrderItems: function () {
            this.orderItems.forEach(function (orderItem) {
                this.removeOrderItem(orderItem);
            }.bind(this));
            $('#remove-all-items-modal').modal('hide');
        },
        capitalize: function (string) {
            return string[0].toUpperCase() + string.slice(1);
        },
        addAreaOptionGroups: function (orderItems) {
            orderItems.forEach(function (orderItem) {
                var orderItemId = orderItem.metadata.uuid;
                var domNodeInserted = false;
                document.addEventListener("DOMNodeInserted", function (event) {
                    var target = event.srcElement || event.target;
                    var elements = $(target).find("#arealist-" + orderItemId);

                    if (elements.length > 0 && !domNodeInserted) {
                        var areaList = $(elements[0]).find("ul.dropdown-menu");
                        var areaListItems = areaList.children("li");
                        areaTypes = orderItem.codelists.areaTypes;
                        var indexCount = 0;
                        areaTypes.forEach(function (areaType) {
                            $(areaListItems[indexCount]).prepend("<span class='area-list-heading'>" + areaType.name + "</span>");
                            indexCount += areaType.numberOfItems;
                        })
                        domNodeInserted = true;
                    }
                });
            })
        },
        selectFromMap: function (orderItem) {
            loadMap(orderItem);
            $('#norgeskartmodal #setcoordinates').attr('uuid', orderItem.metadata.uuid);
        },
        orderItemHasCoordinates: function (orderItem) {
            return (orderItem.codelists.coordinates !== undefined) ? true : false;
        },
        orderHasCoordinates: function () {
            var hasCoordinates = false;
            this.orderItems.forEach(function (orderItem) {
                if (this.orderItemHasCoordinates(orderItem)) hasCoordinates = true;
            }.bind(this));
            return hasCoordinates;
        },
        emailAddressIsValid: function (email) {
            var regex = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            return regex.test(email);
        },
        isEmpty: function (item) {
            return (item == null || item == undefined || item.length == 0);
        },
        allRequiredProjectionAndFormatFieldsIsNotEmpty: function (orderItems) {
            var orderItemsIsValid = true;
            orderItems.forEach(function (orderItem) {
                if (orderItem.projectionAndFormatIsRequired) {
                    if (this.isEmpty(orderItem.codelists.selectedProjections) || this.isEmpty(orderItem.codelists.selectedFormats)) {
                        orderItemsIsValid = false;
                    }
                }
            }.bind(this));
            return orderItemsIsValid;
        },
        formIsValid: function () {
            var emailFieldNotEmpty = (this.email !== "") ? true : false;
            var emailAddressIsValid = this.emailAddressIsValid(this.email);
            var projectionAndFormatFieldsIsValid = this.allRequiredProjectionAndFormatFieldsIsNotEmpty(this.orderItems);
            var emailRequired = this.emailRequired;
            var formIsValid = ((emailFieldNotEmpty && emailRequired && emailAddressIsValid) || (!emailRequired)) ? true : false;
            return formIsValid;
        },
        projectionAndFormatIsRequired: function (orderItem) {
            var required = this.orderItemHasCoordinates(orderItem);
            return required;
        }
    }
});
