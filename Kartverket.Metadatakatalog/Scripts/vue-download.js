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

function updateChosen() {
    $(".chosen-select").trigger("chosen:updated");
}

function getOrderItemName(uuid) {
    var metadata = JSON.parse(localStorage.getItem(uuid + ".metadata"));
    return metadata.name;
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

function getAvailableProjections(itemProjections, projectionsArray) {
    itemProjections.forEach(function (itemProjection) {
        var inArray = false;
        projectionsArray.forEach(function (availableProjection) {
            itemProjection.selected = false;
            if (itemProjection.code == availableProjection.code) inArray = true;
        });
        if (!inArray) {
            projectionsArray.push(itemProjection);
        }
    });
    return projectionsArray;
}

function getAvailableFormats(itemFormats, formatsArray) {
    itemFormats.forEach(function (itemFormat) {
        var inArray = false;
        formatsArray.forEach(function (availableFormat) {
            itemFormat.selected = false;
            if (itemFormat.name == availableFormat.name) inArray = true;
        });
        if (!inArray) {
            formatsArray.push(itemFormat);
        }
    });
    return formatsArray;
}

function updateLocalStorageOrderItem(vnode, binding) {
    var uuid = (vnode.data.attrs.uuid !== undefined) ? vnode.data.attrs.uuid : false;
    var name = (binding.expression !== undefined) ? binding.expression : false;
    var value = (binding.value !== undefined) ? binding.value : false;
    if (uuid && name && value) {
        localStorage[uuid + "." + name] = JSON.stringify(value)
    }
}

$(document).on("click", ".custom-select", function () {
    updateChosen();
});



showLoadingAnimation("Laster inn kurv");


$(window).load(function () {
    hideLoadingAnimation();
    updateChosen();
});


$(document).on("click", ".chosen-results li", function () {
    updateChosen();
});

Vue.directive('chosen', {
    twoWay: true, // note the two-way binding
    bind: function (el, binding, vnode) {
        $(el)
            .change(function (ev, nv) {
                if (vnode.data.attrs.parent !== undefined) {
                    var orderItem = vnode.data.attrs.parent;
                    if (vnode.data.attrs.listname == "area") {
                        mainVueModel.resetProjectionSelections(orderItem);
                        mainVueModel.resetFormatSelections(orderItem);
                    }
                }
                // two-way set
                var ref = el.selectedOptions;
                var indexesForSelectedOptions = [];
                var vueNodes = [];

                $(vnode.data.directives).each(function () {
                    vueNodes = (this.name == "chosen") ? this.value : [];
                });

                $(ref).each(function () {
                    indexesForSelectedOptions.push($(this).data("index"));
                });
                var availableProjections = [];
                var availableFormats = [];

                $(vueNodes).each(function (key, val) {
                    if ($.inArray(key, indexesForSelectedOptions) !== -1) {
                        val.selected = true;
                        if (vnode.data.attrs.listname == "area") {
                            if (val.projections !== undefined) {
                                availableProjections = getAvailableProjections(val.projections, availableProjections);
                            }
                            if (val.formats !== undefined) {
                                availableFormats = getAvailableFormats(val.formats, availableFormats);
                            }
                        }

                    } else {
                        val.selected = false;
                    }
                });


                if (vnode.data.attrs.parent !== undefined) {
                    var orderItem = vnode.data.attrs.parent;
                    if (vnode.data.attrs.listname == "area") {
                        orderItem.codelists.availableProjections = availableProjections;
                        orderItem.codelists.availableFormats = availableFormats;
                        orderItem.codelists.projections.forEach(function (projection) {
                            mainVueModel.setAvailableProjection(projection, availableProjections)
                        });
                        orderItem.codelists.formats.forEach(function (format) {
                            mainVueModel.setAvailableFormat(format, availableFormats)
                        });

                    }
                    updateChosen();
                }
            });
    },

});

Vue.component('areaoption', {
    props: ['area'],
    template: '<option v-bind:value="area.code">{{area.name}}</option>'
});

Vue.component('projectionoption', {
    props: ['projection'],
    template: '<option v-bind:value="projection.code">{{projection.name}}</option>'
});

Vue.component('formatoption', {
    props: ['format'],
    template: '<option v-bind:value="format.name">{{format.name}}</option>'
});


var mainVueModel = new Vue({
    el: '#vueContainer',
    data: {
        orderItems: [],
        email: "",
        orderResponse: {}
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
                    orderRequest.order.orderLines.push(
                            {
                                "metadataUuid": orderItem.metadata.uuid,
                                "coordinates": orderItem.codelists.coordinates,
                                "coordinatesystem": orderItem.codelists.coordinatesystem,
                                "areas": this.getSelectedAreas(orderItem.codelists.areas),
                                "projections": this.getSelectedProjections(orderItem.codelists.projections),
                                "formats": this.getSelectedFormats(orderItem.codelists.formats)
                            }
                    );
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
                        "projections": [],
                        "availableProjections": [],
                        "formats": [],
                        "availableFormats": [],
                        "areaTypes": [],
                        "coordinates": "",
                        "coordinatesystem": ""
                    }
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
                                "selected": false,
                                "formats": orderItems[key].codelists.formats,
                                "projections": orderItems[key].codelists.projections
                            }
                        );
                    }
                    $(orderItems[key].codelists.areas).each(function (index, area) {
                        area.selected = (area.selected == undefined) ? area.selected = false : area.selected = false; // TODO Get from localStorage if defined
                        if (area.type !== undefined) {
                            if ($.inArray(area.type, orderItems[key].codelists.areaTypes) == -1) {
                                orderItems[key].codelists.areaTypes.push(area.type);
                            }
                        }

                    })
                }
                if (orderItems[key].codelists.projections) {
                    $(orderItems[key].codelists.projections).each(function (index, projection) {
                        projection.selected = (projection.selected == undefined) ? projection.selected = false : projection.selected = false; // TODO Get from localStorage if defined
                    })
                }
                if (orderItems[key].codelists.formats) {
                    $(orderItems[key].codelists.formats).each(function (index, format) {
                        format.selected = (format.selected == undefined) ? format.selected = false : format.selected = false; // TODO Get from localStorage if defined
                    })
                }

            }.bind(this));
        }
        this.orderItems = orderItems;
    },
    methods: {
        sendRequests: function () {
            this.orderRequests.forEach(function (orderRequest) {
                var responseData = [];
                if (orderRequest.distributionUrl != "") {
                    $.ajax({
                        url: orderRequest.distributionUrl,
                        type: "POST",
                        dataType: 'json',
                        data: JSON.stringify(orderRequest.order),
                        contentType: "application/json",
                        xhrFields: { withCredentials: true },
                        async: true,
                        error: function (jqXHR, textStatus, errorThrown) {
                            showAlert(errorThrown, "danger");
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
                            }
                        }
                    }).done(function () {
                        $("[data-toggle='tooltip']").tooltip();
                    });
                }
                return this.orderResponse = responseData;
            }.bind(this))
        },
        notSelected: function (elements) {
            var selected = true;
            $(elements).each(function () {
                if (this.selected == true) selected = false;
            })
            return selected;
        },
        setAvailableProjection: function (projection, availableProjections) {
            var available = false;
            availableProjections.forEach(function (availableProjection) {
                if (projection.code == availableProjection.code) available = true;
            })
            projection.available = available;
        },
        setAvailableFormat: function (format, availableFormats) {
            var available = false;
            availableFormats.forEach(function (availableFormat) {
                if (format.name == availableFormat.name) available = true;
            })
            format.available = available;
        },
        getSelectedAreas: function (areas) {
            var selectedAreas = [];
            if (areas !== undefined && areas !== "") {
                areas.forEach(function (area) {
                    if (area.selected == true) selectedAreas.push({
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
                    if (projection.selected == true) selectedProjections.push({
                        "code": projection.code,
                        "name": projection.name,
                        "codespace": projection.codespace
                    });
                });
            }
            return selectedProjections;
        },
        setSelectedProjections: function (selectedProjections, projections) {
            selectedProjections.forEach(function (selectedProjection) {
                if ($.inArray(selectedProjection, projections) == -1) {
                    //orderItems[key].codelists.areaTypes.push(area.type);
                }
            });

        },
        getSelectedFormats: function (formats) {
            var selectedFormats = [];
            if (formats !== undefined && formats !== "") {
                formats.forEach(function (format) {
                    if (format.selected == true && format.version !== undefined) {
                        selectedFormats.push({
                            "name": format.name,
                            "version": format.version
                        });
                    }
                    else if (format.selected == true) {
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
        selectFromMap: function (orderItem) {
            loadMap(orderItem);
            $('#norgeskartmodal #setcoordinates').attr('uuid', orderItem.metadata.uuid);
        }
    }
}).$mount('#downloadformVue');

