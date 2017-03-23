﻿
function showAlert(message, colorClass) {
    $('#feedback-alert').attr('class', 'alert alert-dismissible alert-' + colorClass);
    $('#feedback-alert .message').html($('#feedback-alert .message').html() + message + "<br/>");
    $('#feedback-alert').show();
}


$(document).on('focus', '.custom-select-list-input', function () {
    var customSelectListElement = $(this).closest('.custom-select-list');
    var dropdownElement = customSelectListElement.find('.custom-select-list-dropdown');
    dropdownElement.addClass('active');
    dropdownElement.removeClass('transparent');
})

$(document).on('blur', '.custom-select-list-input', function () {
    var inputElement = this;
    var customSelectListElement = $(this).closest('.custom-select-list');
    var dropdownElement = customSelectListElement.find('.custom-select-list-dropdown');
    dropdownElement.addClass("transparent")
    setTimeout(function () {
        if (inputElement !== document.activeElement) {
            dropdownElement.removeClass("active")
            dropdownElement.removeClass("transparent")
        }
    }, 1000);
})

$(document).on('click', '.custom-select-list-input-container', function () {
    $(this).find('.custom-select-list-input').focus();
})

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

var Areas = {
    props: ['available', 'selected', 'master'],
    template: '#areas-template',
    data: function () {
        var data = {
            supportsPolygonSelection: false,
            supportsGridSelection: false
        }
        if (!this.master) {
            data.supportsPolygonSelection = this.$parent.capabilities.supportsPolygonSelection,
            data.supportsGridSelection = this.$parent.capabilities.supportsGridSelection
        }
        return data;
    },
    created: function () {
        if (!this.master && this.$parent.capabilities._links) {
            var areas = [];
            this.$parent.capabilities._links.forEach(function (link) {
                if (link.rel == "http://rel.geonorge.no/download/area") {
                    areas = getJsonData(link.href);
                }


            });
            areas.forEach(function (area) {
                if (this.available[area.type] == undefined) { this.available[area.type] = [] }
                area.hasSelectedProjections = false;
                area.hasSelectedFormats = false;
                area.isSelected = false;
                this.available[area.type].push(area);
            }.bind(this))
        }
        if (this.master) {

        }
    },

    methods: {
        selectArea: function (area) {
            area.isSelected = true;

            //var orderLineObject = (this.master) ? this.$parent : this.$parent;

            this.$parent.updateSelectedAreas();

            this.$parent.updateAvailableProjections();
            this.$parent.updateAvailableFormats();

            this.$parent.updateSelectedProjections();
            this.$parent.updateSelectedFormats();

            this.$parent.validateAreas();
        },
        removeSelectedArea: function (area) {
            area.isSelected = false;

            this.$parent.updateSelectedAreas();

            this.$parent.updateAvailableProjections();
            this.$parent.updateAvailableFormats();

            this.$parent.updateSelectedProjections();
            this.$parent.updateSelectedFormats();

            this.$parent.validateAreas();

        }
    }
};

var Projections = {
    props: ['available', 'selected', 'master'],
    template: '#projections-template',

    methods: {
        selectProjection: function (projection) {
            projection.isSelected = true;
            if (this.master) {
                for (orderLine in this.$parent.allAvailableProjections) {
                    this.$parent.allAvailableProjections[orderLine].forEach(function (availableProjection) {
                        if (availableProjection.code == projection.code) {
                            availableProjection.isSelected = true;
                        }
                    })
                }
            }
            this.$parent.updateSelectedProjections();
            this.$parent.validateAreas();
            this.$parent.updateSelectedAreas();
        },
        removeSelectedProjection: function (projection) {
            projection.isSelected = false;
            if (this.master) {
                for (orderLine in this.$parent.allAvailableProjections) {
                    this.$parent.allAvailableProjections[orderLine].forEach(function (availableProjection) {
                        if (availableProjection.code == projection.code) {
                            availableProjection.isSelected = false;
                        }
                    })
                }
            }
            this.$parent.updateSelectedProjections();
            this.$parent.validateAreas();
            this.$parent.updateSelectedAreas();
        }
    }
};

var Formats = {
    props: ['available', 'selected', 'master'],
    template: '#formats-template',

    methods: {
        selectFormat: function (format) {
            format.isSelected = true;
            if (this.master) {
                for (orderLine in this.$parent.allAvailableFormats) {
                    this.$parent.allAvailableFormats[orderLine].forEach(function (availableFormat) {
                        if (availableFormat.name == format.name) {
                            availableFormat.isSelected = true;
                        }
                    })
                }
            }
            this.$parent.updateSelectedFormats();
            this.$parent.validateAreas();
            this.$parent.updateSelectedAreas();
        },
        removeSelectedFormat: function (format) {
            format.isSelected = false;
            if (this.master) {
                for (orderLine in this.$parent.allAvailableFormats) {
                    this.$parent.allAvailableFormats[orderLine].forEach(function (availableFormat) {
                        if (availableFormat.name == format.name) {
                            availableFormat.isSelected = false;
                        }
                    })
                }
            }
            this.$parent.updateSelectedFormats();
            this.$parent.validateAreas();
            this.$parent.updateSelectedAreas();
        }
    }
};



var OrderLine = {
    props: ['metadata', 'capabilities', 'availableAreas', 'availableProjections', 'availableFormats', 'selectedAreas', 'selectedProjections', 'selectedFormats', 'orderLineErrors'],
    template: '#order-line-template',
    data: function () {
        var data = {
            expanded: false
        }
        return data;
    },
    computed: {
        hasErrors: function () {
            var hasErrors = false;
            if (this.orderLineErrors !== undefined && Object.keys(this.orderLineErrors).length) {
                for (errorType in this.orderLineErrors) {
                    if (this.orderLineErrors[errorType].length) {
                        hasErrors = true;
                    }
                }
            }
            return hasErrors;
        },
        numberOfErrors: function () {
            var numberOfErrors = 0;
            if (this.orderLineErrors !== undefined && Object.keys(this.orderLineErrors).length) {
                for (errorType in this.orderLineErrors) {
                    numberOfErrors += this.orderLineErrors[errorType].length;
                }
            }
            return numberOfErrors;
        }
    },
    methods: {
        filterOptionList: function (optionListId, inputValue) {
            var dropdownListElements = document.getElementsByClassName(optionListId);
            var filter = inputValue.toUpperCase();
            for (var listIndex = 0; listIndex < dropdownListElements.length; listIndex++) {
                var listItems = dropdownListElements[listIndex].getElementsByTagName('li');
                for (var i = 0; i < listItems.length; i++) {
                    if (listItems[i].innerHTML.toUpperCase().indexOf(filter) > -1) {
                        listItems[i].style.display = "";
                    } else {
                        listItems[i].style.display = "none";
                    }
                }
            }
        },
        updateSelectedAreas: function () {
        },
        updateAvailableProjections: function () {
        },
        updateSelectedProjections: function () {
        },
        updateAvailableFormats: function () {
        },
        updateSelectedFormats: function () {
        },

        hasSelectedProjections: function (area) {
        },
        hasSelectedFormats: function (area) {
        },

    },
    components: {
        'areas': Areas,
        'projections': Projections,
        'formats': Formats
    },
};

var MasterOrderLine = {
    props: ['allAvailableAreas', 'allAvailableProjections', 'allAvailableFormats', 'allSelectedAreas', 'allSelectedProjections', 'allSelectedFormats', 'allOrderLineErrors'],
    data: function () {
        var data = {
            availableAreas: {},
            availableProjections: [],
            availableFormats: [],

            selectedAreas: [],
            selectedProjections: [],
            selectedFormats: []
        }
        return data;
    },
    created: function () {
        for (orderLine in this.allAvailableAreas) {
            for (areaType in this.allAvailableAreas[orderLine]) {
                this.allAvailableAreas[orderLine][areaType].forEach(function (area) {
                    if (this.availableAreas[areaType] == undefined) {
                        this.availableAreas[areaType] = [];
                    }

                    area.orderLineUuids = [];
                    area.orderLineUuids.push(orderLine);

                    if (area.allAvailableProjections == undefined) { area.allAvailableProjections = {} };
                    if (area.allAvailableProjections[orderLine] == undefined) { area.allAvailableProjections[orderLine] = [] };
                    area.allAvailableProjections[orderLine] = area.projections;

                    if (area.allAvailableFormats == undefined) { area.allAvailableFormats = {} };
                    if (area.allAvailableFormats[orderLine] == undefined) { area.allAvailableFormats[orderLine] = [] };
                    area.allAvailableFormats[orderLine] = area.formats;


                    var areaIsAllreadyAddedInfo = this.isAllreadyAdded(this.availableAreas[areaType], area, "code");

                    if (!areaIsAllreadyAddedInfo.added) {
                        this.availableAreas[areaType].push(area);
                    } else {
                        var orderLineUuidIsAdded = false

                        if (!orderLineUuidIsAdded) {

                            // Add available projections to area
                            if (this.availableAreas[areaType][areaIsAllreadyAddedInfo.position].allAvailableProjections[orderLine] == undefined) {
                                this.availableAreas[areaType][areaIsAllreadyAddedInfo.position].allAvailableProjections[orderLine] = [];
                            }
                            this.availableAreas[areaType][areaIsAllreadyAddedInfo.position].allAvailableProjections[orderLine] = area.projections;


                            // Add available formats to area
                            if (this.availableAreas[areaType][areaIsAllreadyAddedInfo.position].allAvailableFormats[orderLine] == undefined) {
                                this.availableAreas[areaType][areaIsAllreadyAddedInfo.position].allAvailableFormats[orderLine] = [];
                            }
                            this.availableAreas[areaType][areaIsAllreadyAddedInfo.position].allAvailableFormats[orderLine] = area.formats;

                        }
                    }
                }.bind(this))
            } 
        }
        this.validateAreas();
        this.updateSelectedAreas();
    },
    methods: {
        isAllreadyAdded: function (array, item, propertyToCompare) {
            var isAllreadyAdded = {
                added: false,
                position: 0
            };
            array.forEach(function (arrayItem, index) {
                if (this.readProperty(arrayItem, propertyToCompare) == this.readProperty(item, propertyToCompare)) {
                    isAllreadyAdded.added = true
                    isAllreadyAdded.position = index;
                };
            }.bind(this))
            return isAllreadyAdded;
        },
        readProperty: function (obj, prop) {
            return obj[prop];
        },
        filterOptionList: function (optionListId, inputValue) {
            var dropdownListElements = document.getElementsByClassName(optionListId);
            var filter = inputValue.toUpperCase();
            for (var listIndex = 0; listIndex < dropdownListElements.length; listIndex++) {
                var listItems = dropdownListElements[listIndex].getElementsByTagName('li');
                for (var i = 0; i < listItems.length; i++) {
                    if (listItems[i].innerHTML.toUpperCase().indexOf(filter) > -1) {
                        listItems[i].style.display = "";
                    } else {
                        listItems[i].style.display = "none";
                    }
                }
            }
        },
        updateSelectedAreas: function () {
            var allSelectedAreas = {};
            var selectedAreas = [];
            for (areaType in this.availableAreas) {
                this.availableAreas[areaType].forEach(function (area) {
                    if (area.isSelected) {
                        area.orderLineUuids.forEach(function (orderLineUuid) {
                            if (allSelectedAreas[orderLineUuid] == undefined) { allSelectedAreas[orderLineUuid] = [] }
                            allSelectedAreas[orderLineUuid].push(area);

                            var isAllreadyAddedInfo = this.isAllreadyAdded(selectedAreas, area, "code");
                            if (!isAllreadyAddedInfo.added) {
                                selectedAreas.push(area);
                            }
                        }.bind(this))
                        /* if (area.projections.length == 1) {
                             area.projections[0].isSelected = true;
                         }
                         if (area.formats.length == 1) {
                             area.formats[0].isSelected = true;
                         }*/
                    }
                }.bind(this));
            }
            this.$parent.masterOrderLine.allSelectedAreas = allSelectedAreas;
            this.selectedAreas = selectedAreas;

        },
        updateAvailableProjections: function () {
            var availableProjections = [];
            var allAvailableProjections = {}
            if (this.$parent.masterOrderLine.allSelectedAreas !== undefined) {
                for (orderLine in this.$parent.masterOrderLine.allSelectedAreas) {

                    this.$parent.masterOrderLine.allSelectedAreas[orderLine].forEach(function (selectedArea) {
                        if (selectedArea.allAvailableProjections !== undefined) {

                            // All available projections for orderLine
                            selectedArea.allAvailableProjections[orderLine].forEach(function (projection) {
                                if (projection.isSelected == undefined) { projection.isSelected = false }

                                // Update availableProjections array
                                var isAllreadyAddedInfo = this.isAllreadyAdded(availableProjections, projection, "code");
                                if (!isAllreadyAddedInfo.added) {
                                    availableProjections.push(projection);
                                }

                                // Update allAvailableProjections object
                                if (allAvailableProjections[orderLine] == undefined) { allAvailableProjections[orderLine] = [] }
                                var isAllreadyAddedInfo = this.isAllreadyAdded(allAvailableProjections[orderLine], projection, "code");
                                if (!isAllreadyAddedInfo.added) {
                                    allAvailableProjections[orderLine].push(projection);
                                }
                            }.bind(this))

                        }
                    }.bind(this))

                }
            }
            this.availableProjections = availableProjections;
            this.$parent.masterOrderLine.allAvailableProjections = allAvailableProjections;


        },
        updateAvailableFormats: function () {
            var availableFormats = [];
            var allAvailableFormats = {};
            if (this.$parent.masterOrderLine.allSelectedAreas !== undefined) {
                for (orderLine in this.$parent.masterOrderLine.allSelectedAreas) {

                    this.$parent.masterOrderLine.allSelectedAreas[orderLine].forEach(function (selectedArea) {
                        if (selectedArea.allAvailableFormats !== undefined) {

                            // All available formats for orderLine
                            selectedArea.allAvailableFormats[orderLine].forEach(function (format) {
                                if (format.isSelected == undefined) { format.isSelected = false }

                                // Update availableFormats array
                                var isAllreadyAddedInfo = this.isAllreadyAdded(availableFormats, format, "name");
                                if (!isAllreadyAddedInfo.added) {
                                    availableFormats.push(format);
                                }

                                // Update allAvailableFormats object
                                if (allAvailableFormats[orderLine] == undefined) { allAvailableFormats[orderLine] = [] }
                                var isAllreadyAddedInfo = this.isAllreadyAdded(allAvailableFormats[orderLine], format, "name");
                                if (!isAllreadyAddedInfo.added) {
                                    allAvailableFormats[orderLine].push(format);
                                }
                            }.bind(this))

                        }
                    }.bind(this))

                }
            }
            this.availableFormats = availableFormats;
            this.$parent.masterOrderLine.allAvailableFormats = allAvailableFormats;
        },

        updateSelectedProjections: function () {
            var allSelectedProjections = {};
            var selectedProjections = [];
            for (orderLine in this.$parent.masterOrderLine.allAvailableProjections) {
                this.$parent.masterOrderLine.allAvailableProjections[orderLine].forEach(function (availableProjection) {

                    if (availableProjection.isSelected) {
                        // Update availableProjections array
                        var isAllreadyAddedInfo = this.isAllreadyAdded(selectedProjections, availableProjection, "code");
                        if (!isAllreadyAddedInfo.added) {
                            selectedProjections.push(availableProjection);
                        }

                        // Update allAvailableProjections object
                        if (allSelectedProjections[orderLine] == undefined) { allSelectedProjections[orderLine] = [] }
                        var isAllreadyAddedInfo = this.isAllreadyAdded(allSelectedProjections[orderLine], availableProjection, "code");
                        if (!isAllreadyAddedInfo.added) {
                            allSelectedProjections[orderLine].push(availableProjection);
                        }
                    }

                }.bind(this))
            }

            this.$parent.masterOrderLine.allSelectedProjections = allSelectedProjections;
            this.selectedProjections = selectedProjections;
        },

        updateSelectedFormats: function () {
            var allSelectedFormats = {};
            var selectedFormats = [];
            for (orderLine in this.$parent.masterOrderLine.allAvailableFormats) {
                this.$parent.masterOrderLine.allAvailableFormats[orderLine].forEach(function (availableFormats) {

                    if (availableFormats.isSelected) {
                        // Update availableFormats array
                        var isAllreadyAddedInfo = this.isAllreadyAdded(selectedFormats, availableFormats, "name");
                        if (!isAllreadyAddedInfo.added) {
                            selectedFormats.push(availableFormats);
                        }

                        // Update allAvailableFormats object
                        if (allSelectedFormats[orderLine] == undefined) { allSelectedFormats[orderLine] = [] }
                        var isAllreadyAddedInfo = this.isAllreadyAdded(allSelectedFormats[orderLine], availableFormats, "name");
                        if (!isAllreadyAddedInfo.added) {
                            allSelectedFormats[orderLine].push(availableFormats);
                        }
                    }

                }.bind(this))
            }

            this.$parent.masterOrderLine.allSelectedFormats = allSelectedFormats;
            this.selectedFormats = selectedFormats;
        },
        hasSelectedProjections: function (area, orderLine) {
            var hasSelectedProjections = false;

            if (area.allAvailableProjections[orderLine] && area.allAvailableProjections[orderLine].length) {
                area.allAvailableProjections[orderLine].forEach(function (availableProjection) {
                    if (this.$parent.masterOrderLine.allSelectedProjections[orderLine] !== undefined && this.$parent.masterOrderLine.allSelectedProjections[orderLine].length) {
                        this.$parent.masterOrderLine.allSelectedProjections[orderLine].forEach(function (selectedProjection) {
                            if (selectedProjection.code == availableProjection.code) {
                                hasSelectedProjections = true
                            }
                        }.bind(this))
                    }
                }.bind(this))
            }
            if (!hasSelectedProjections) {
                var errorMessage = "Støttet projeksjon for " + area.name + " mangler";
                this.$parent.masterOrderLine.allOrderLineErrors[orderLine]["projection"].push(errorMessage);
            }
            return hasSelectedProjections;
        },
        hasSelectedFormats: function (area, orderLine) {
            var hasSelectedFormats = false;

            if (area.allAvailableFormats[orderLine] && area.allAvailableFormats[orderLine].length) {
                area.allAvailableFormats[orderLine].forEach(function (availableFormat) {
                    if (this.$parent.masterOrderLine.allSelectedFormats[orderLine] !== undefined && this.$parent.masterOrderLine.allSelectedFormats[orderLine].length) {
                        this.$parent.masterOrderLine.allSelectedFormats[orderLine].forEach(function (selectedFormat) {
                            if (selectedFormat.name == availableFormat.name) {
                                hasSelectedFormats = true
                            }
                        }.bind(this))
                    }
                }.bind(this))
            }
            if (!hasSelectedFormats) {
                var errorMessage = "Støttet format for " + area.name + " mangler";
                this.$parent.masterOrderLine.allOrderLineErrors[orderLine]["format"].push(errorMessage);
            }
            return hasSelectedFormats;
        },
        removeAllValidationErrors: function () {
            for (orderLine in this.$parent.masterOrderLine.allOrderLineErrors) {
                for (errorType in this.$parent.masterOrderLine.allOrderLineErrors[orderLine]) {
                    this.$parent.masterOrderLine.allOrderLineErrors[orderLine][errorType] = [];
                }
            }
        },
        validateAreas: function () {

            for (orderLine in this.$parent.masterOrderLine.allAvailableAreas) {
                this.$parent.masterOrderLine.allOrderLineErrors[orderLine] = {};
                this.$parent.masterOrderLine.allOrderLineErrors[orderLine]["projection"] = [];
                this.$parent.masterOrderLine.allOrderLineErrors[orderLine]["format"] = [];
                this.$parent.masterOrderLine.allOrderLineErrors[orderLine]["area"] = [];
                if (this.$parent.masterOrderLine.allSelectedAreas[orderLine] !== undefined && this.$parent.masterOrderLine.allSelectedAreas[orderLine].length) {

                    
                        this.$parent.masterOrderLine.allSelectedAreas[orderLine].forEach(function (selectedArea) {
                            selectedArea.hasSelectedProjections = this.hasSelectedProjections(selectedArea, orderLine);
                            selectedArea.hasSelectedFormats = this.hasSelectedFormats(selectedArea, orderLine);
                        }.bind(this));
                    
                } else {
                    this.$parent.masterOrderLine.allOrderLineErrors[orderLine]["area"] = ["Datasett mangler valgt område"];
                }
            }
            setTimeout(function () {
                $("[data-toggle='tooltip']").tooltip();
            }, 300);
        }


    },
    template: '#master-order-line-template',
    components: {
        'areas': Areas,
        'projections': Projections,
        'formats': Formats
    }
}



Vue.config.debug = true;



var mainVueModel = new Vue({
    el: '#vueContainer',
    data: {
        orderLines: [],
        email: "",
        orderResponse: {},
        emailRequired: false,

        masterOrderLine: {
            allAvailableAreas: {},
            allAvailableProjections: {},
            allAvailableFormats: {},
            allSelectedAreas: {},
            allSelectedProjections: {},
            allSelectedFormats: {},
            allOrderLineErrors: {}
        }
    },
    computed: {
        orderRequests: function () {
            var orderRequests = [];
            var orderLinesGrouped = this.groupBy(this.$children, function (orderLine) {
                return [orderLine.metadata.distributionUrl]
            });
            orderLinesGrouped.forEach(function (orderLineGroup) {
                var orderRequest = {
                    "distributionUrl": orderLineGroup[0].metadata.orderDistributionUrl,
                    "order": {
                        "email": this.email,
                        "orderLines": []
                    }
                }

                var orderLines = [];
                orderLineGroup.forEach(function (orderLine) {
                    var orderLineObject = {
                        "metadataUuid": orderLine.metadata.uuid
                    }
                    if (this.getSelectedAreas(orderLine.selectedAreas).length) {
                        orderLineObject.areas = this.getSelectedAreas(orderLine.selectedAreas);
                    }
                    if (this.getSelectedProjections(orderLine.selectedProjections).length) {
                        orderLineObject.projections = this.getSelectedProjections(orderLine.selectedProjections);
                    }
                    if (this.getSelectedFormats(orderLine.selectedFormats).length) {
                        orderLineObject.formats = this.getSelectedFormats(orderLine.selectedFormats);
                    }
                    orderRequest.order.orderLines.push(orderLineObject);
                }.bind(this));

                orderRequests.push(orderRequest);
            }.bind(this))
            return orderRequests;
        }
    },
    created: function () {
        var defaultUrl = "https://nedlasting.geonorge.no/api/capabilities/";
        var orderItemsJson = (localStorage["orderItems"] != null) ? JSON.parse(localStorage["orderItems"]) : [];
        var orderLines = [];
        if (orderItemsJson != []) {
            $(orderItemsJson).each(function (key, val) {
                var metadata = (localStorage[val + ".metadata"] !== undefined) ? JSON.parse(localStorage[val + ".metadata"]) : "";
                var apiUrl = (metadata.distributionUrl !== undefined) ? metadata.distributionUrl : defaultUrl;

                orderLines[key] = {
                    "metadata": metadata,
                    "capabilities": getJsonData(apiUrl + val),
                    "projectionAndFormatIsRequired": false
                }

                var uuid = metadata.uuid;

                this.masterOrderLine.allAvailableProjections[uuid] = {};
                this.masterOrderLine.allAvailableFormats[uuid] = {};

                if (orderLines[key].capabilities._links !== undefined && orderLines[key].capabilities._links.length) {
                    orderLines[key].capabilities._links.forEach(function (link) {
                        if (link.rel == "http://rel.geonorge.no/download/order") {
                            orderLines[key].metadata.orderDistributionUrl = link.href;
                        }
                        if (link.rel == "http://rel.geonorge.no/download/can-download") {
                            orderLines[key].metadata.canDownloadUrl = link.href;
                        }
                        if (link.rel == "http://rel.geonorge.no/download/area") {
                            var availableAreas = getJsonData(link.href);
                            this.masterOrderLine.allAvailableAreas[uuid] = {};

                            availableAreas.forEach(function (availableArea) {
                                if (this.masterOrderLine.allAvailableAreas[uuid][availableArea.type] == undefined) {
                                    this.masterOrderLine.allAvailableAreas[uuid][availableArea.type] = [];
                                }
                                this.masterOrderLine.allAvailableAreas[uuid][availableArea.type].push(availableArea);
                            }.bind(this))
                        }
                        if (link.rel == "http://rel.geonorge.no/download/projection") {
                            orderLines[key].defaultProjections = getJsonData(link.href);
                        }
                        if (link.rel == "http://rel.geonorge.no/download/format") {
                            orderLines[key].defaultFormats = getJsonData(link.href);
                        }
                    }.bind(this))
                }
                /*
                
                
                
                if (link.rel == "http://rel.geonorge.no/download/can-download") {
                    orderItems[key].metadata.canDownloadUrl = link.href;
                }
                */

                var distributionUrl = (orderLines[key].metadata.distributionUrl !== undefined) ? orderLines[key].metadata.distributionUrl : "";


                orderLines[key].capabilities.supportsGridSelection = (orderLines[key].capabilities.mapSelectionLayer !== undefined && orderLines[key].capabilities.mapSelectionLayer !== "") ? true : false;

                /*   if (orderItems[key].codelists.areas) {
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
                   }*/
            }.bind(this));
        }
        this.orderLines = orderLines;
        this.addAreaOptionGroups(orderLines);

    },
    components: {
        'orderLine': OrderLine,
        'masterOrderLine': MasterOrderLine
    },
    methods: {

        cloneSelectedProperties: function (selectedOrderLineIndex) {
            selectedAreas = this.$children[selectedOrderLineIndex].selectedAreas;
            selectedProjections = this.$children[selectedOrderLineIndex].selectedProjections;
            selectedFormats = this.$children[selectedOrderLineIndex].selectedFormats;
            this.$children.forEach(function (orderLine, index) {
                if (index !== selectedOrderLineIndex) {

                    for (areaType in orderLine.availableAreas) {
                        orderLine.availableAreas[areaType].forEach(function (area) {
                            selectedAreas.forEach(function (selectedArea) {
                                if (area.code == selectedArea.code) area.isSelected = true;
                            })
                        })
                    }

                    orderLine.updateSelectedAreas();
                    orderLine.updateAvailableProjections();
                    orderLine.updateAvailableFormats();


                    for (projectionCode in orderLine.availableProjections) {
                        var projection = orderLine.availableProjections[projectionCode];
                        selectedProjections.forEach(function (selectedProjection) {
                            if (projection.code == selectedProjection.code) projection.isSelected = true;
                        })
                    }
                    orderLine.updateSelectedProjections();


                    for (formatName in orderLine.availableFormats) {
                        var format = orderLine.availableFormats[formatName];
                        selectedFormats.forEach(function (selectedFormat) {
                            if (format.name == selectedFormat.name) format.isSelected = true;
                        })
                    }
                    orderLine.updateSelectedFormats();

                    orderLine.validateAreas();
                }

            })

        },
        updateAllOrderLineFields: function () {
            this.$children.forEach(function (orderLine, index) {
                orderLine.updateSelectedAreas();
                orderLine.updateAvailableProjections();
                orderLine.updateAvailableFormats();
            });
        },
        clearSelectedProperties: function (selectedOrderLineIndex) {
            selectedAreas = this.$children[selectedOrderLineIndex].selectedAreas;
            selectedProjections = this.$children[selectedOrderLineIndex].selectedProjections;
            selectedFormats = this.$children[selectedOrderLineIndex].selectedFormats;

            selectedAreas.forEach(function (selectedArea) {
                selectedArea.isSelected = false;
            })
            this.$children[selectedOrderLineIndex].updateSelectedAreas();

            selectedProjections.forEach(function (selectedProjection) {
                selectedProjection.isSelected = false;
            })
            this.$children[selectedOrderLineIndex].updateSelectedProjections();

            selectedFormats.forEach(function (selectedFormat) {
                selectedFormat.isSelected = false;
            })
            this.$children[selectedOrderLineIndex].updateSelectedFormats();
        },
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
                    }
                });

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
            orderItem.codelists.availableProjections = availableProjections;
            orderItem.codelists.selectedFormats = [];
            orderItem.codelists.availableFormats = availableFormats;
            this.emailRequired = this.orderHasCoordinates();

        },

        selectArea: function (orderItem, area) {
            orderItem.codelists.selectedAreas.push(area);
        },
        removeSelectedArea: function (orderItem, area) {
            var code = area.code;
            var selectedAreas = orderItem.codelists.selectedAreas;
            var newSelectedAreas = selectedAreas.filter(function (obj) {
                return obj.code !== code;
            });
            orderItem.codelists.selectedAreas = newSelectedAreas;
        },

        getAreasByType: function (areas, type) {
            var areasWithType = [];
            areas.forEach(function (area) {
                if (area.type == type) areasWithType.push(area);
            });
            return areasWithType;
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
            this.orderLines = this.orderLines.filter(function (obj) {
                return obj.metadata.uuid !== uuid;
            });
            this.removeFromLocalStorage(uuid);
        },
        removeAllOrderItems: function () {
            this.orderLines.forEach(function (orderItem) {
                this.removeOrderItem(orderItem);
            }.bind(this));
            $('#remove-all-items-modal').modal('hide');
        },
        capitalize: function (string) {
            return string[0].toUpperCase() + string.slice(1);
        },
        addAreaOptionGroups: function (orderLines) {
            orderLines.forEach(function (orderItem) {
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
            this.orderLines.forEach(function (orderItem) {
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
        allRequiredProjectionAndFormatFieldsIsNotEmpty: function (orderLines) {
            var orderItemsIsValid = true;
            orderLines.forEach(function (orderItem) {
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
            var projectionAndFormatFieldsIsValid = this.allRequiredProjectionAndFormatFieldsIsNotEmpty(this.orderLines);
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
