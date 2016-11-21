function showAlert(message, colorClass) {
    $('#feedback-alert').attr('class', 'alert alert-dismissible alert-' + colorClass);
    $('#feedback-alert .message').html($('#feedback-alert .message').html() + message);
    $('#feedback-alert').show();
}

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

function setAvailableProjections(itemProjections, projectionsArray) {
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

function setAvailableFormats(itemFormats, formatsArray) {
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

Vue.directive('chosen', {
    twoWay: true, // note the two-way binding
    bind: function (el, binding, vnode) {
        $(el)
            .change(function (ev, nv) {
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
                        if (val.projections !== undefined && vnode.data.attrs.parent !== undefined) {
                            availableProjections = setAvailableProjections(val.projections, availableProjections);
                        }
                        if (val.formats !== undefined && vnode.data.attrs.parent !== undefined) {
                            availableFormats = setAvailableFormats(val.formats, availableFormats);
                        }

                    } else {
                        val.selected = false;
                    }
                });
                if (vnode.data.attrs.parent !== undefined) {
                    var orderItem = vnode.data.attrs.parent;
                    orderItem.codelists.availableProjections = availableProjections;
                    orderItem.codelists.availableFormats = availableFormats;
                }
                //   updateLocalStorageOrderItem(vnode, binding);
            });
    },

});

Vue.component('areaoption', {
    props: ['area'],
    template: '<option v-bind:value="area.code">{{area.name}}</option>'
});

Vue.component('projectionoption', {
    props: ['projection'],
    template: '<option v-bind:value="projection.code" >{{projection.name}}</option>'
});

Vue.component('formatoption', {
    props: ['format'],
    template: '<option v-bind:value="format.name" >{{format.name}}</option>'
});


var app = new Vue({
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
        //loadData();
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
                        "areas": (localStorage[val + ".orderItem.codelists.areas"] !== undefined) ? JSON.parse(localStorage[val + ".orderItem.codelists.areas"]) : [],
                        "projections": (localStorage[val + ".orderItem.codelists.projections"] !== undefined) ? JSON.parse(localStorage[val + ".orderItem.codelists.projections"]) : [],
                        "availableProjections": [],
                        "formats": (localStorage[val + ".orderItem.codelists.formats"] !== undefined) ? JSON.parse(localStorage[val + ".orderItem.codelists.formats"]) : [],
                        "availableFormats": [],
                        "areaTypes": [],
                        "coordinates": "",
                        "coordinatesystem": ""
                    }
                }

                var distributionUrl = (orderItems[key].metadata.distributionUrl !== undefined) ? orderItems[key].metadata.distributionUrl : "";
                orderItems[key].metadata.orderDistributionUrl = this.getOrderDistributionUrl(distributionUrl);

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
                    });
                }
                orderItems[key].capabilities.supportsGridSelection = (orderItems[key].capabilities.mapSelectionLayer !== undefined && orderItems[key].capabilities.mapSelectionLayer !== "") ? true : false;

                if (orderItems[key].codelists.areas) {
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
                    });
                }
                return this.orderResponse = responseData;
            }.bind(this))
        },
        populateProjectionsAndFormats: function (orderItem) {
            var selectedAreas = [];
            orderItem.codelists.areas.forEach(function (index, area) {
                if (area.selected) selectedAreas.push(area);
            });
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
                    if (area.selected == true) selectedAreas.push({
                        "code": area.code,
                        "name": area.name,
                        "type": area.type
                    });
                });
            }
            return selectedAreas;
        },
        getOrderDistributionUrl: function (distributionUrl) {
            return distributionUrl.replace("capabilities/", "v2/order");
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
        capitalize: function (string) {
            return string[0].toUpperCase() + string.slice(1);
        },
        selectFromMap: function (item) {
            var uuid = item.metadata.uuid;
            var orderItemSelectOmraader = $('select[name=' + uuid + '-areas]');
            var supportspolygonfixedselection = item.capabilities.supportsGridSelection;
            var areatype = item.codelists.areaTypes[0];
            var mapselectionlayer = item.capabilities.mapSelectionLayer;
            //if (!mapLoaded)
            loadMap(uuid, supportspolygonfixedselection, areatype, mapselectionlayer, item);
            //mapLoaded = true; //Need to load each time to set coverageMap.
            $('#norgeskartmodal #setcoordinates').attr('uuid', uuid);
        }
    }
}).$mount('#downloadformVue');


