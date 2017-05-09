$(document).on("click", "#remove-all-items", function () {
    $('#remove-all-items-modal').modal('show')
});


function fixUrl(urlen) {
    urlJson = urlen.replace("%3F", "?");
    return urlJson;
}


function getOrderItemName(uuid) {
    var metadata = JSON.parse(localStorage.getItem(uuid + ".metadata"));
    return metadata !== null && metadata.name !== undefined ? metadata.name : "";
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
                showAlert("Kunne ikke legge til " + name + " i kurv. Feilmelding: " + errorThrown + "<br/>", "danger");
                returnData = "error";
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

var MapSelect = {
    props: ['mapData', 'mapSrc', 'master'],
    template: '#map-template',
}


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
        else {
            data.supportsPolygonSelection = this.$parent.masterSupportsPolygonSelection();
        }
        return data;
    },

    methods: {
        selectArea: function (area) {
            if (this.master) {
                for (orderLineUuid in this.$root.masterOrderLine.allAvailableAreas) {
                    if (this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type] !== undefined) {
                        this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type].forEach(function (availableArea, index) {
                            if (availableArea.code == area.code) {
                                this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type][index].isSelected = true;

                                var isAllreadyAddedInfo = this.$root.isAllreadyAdded(this.$parent.selectedAreas, area, "code");
                                if (!isAllreadyAddedInfo.added) {
                                    this.$parent.selectedAreas.push(area);
                                }
                            }
                        }.bind(this));
                    }
                }
                this.$root.updateSelectedAreasForAllOrderLines();

            } else {
                var orderLineUuid = this.$parent.metadata.uuid;
                if (this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type] !== undefined) {
                    this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type].forEach(function (availableArea, index) {
                        if (availableArea.code == area.code) {
                            this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type][index].isSelected = true;

                        }
                    }.bind(this));
                }
                this.$root.updateSelectedAreasForSingleOrderLine(orderLineUuid);
            }

            this.$root.validateAreas();
        },
        removeSelectedArea: function (area) {
            if (this.master) {
                for (orderLineUuid in this.$root.masterOrderLine.allAvailableAreas) {
                    if (this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type] !== undefined) {
                        this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type].forEach(function (availableArea, index) {
                            if (availableArea.code == area.code) {
                                this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type][index].isSelected = false;
                                this.$parent.selectedAreas.forEach(function (selectedArea, index) {
                                    if (selectedArea.code == area.code) {
                                        this.$parent.selectedAreas.splice(index, 1)
                                    }
                                }.bind(this));
                            }
                        }.bind(this));
                    }
                }
                this.$root.updateSelectedAreasForAllOrderLines();

            } else {
                var orderLineUuid = this.$parent.metadata.uuid;
                if (this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type] !== undefined) {
                    this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type].forEach(function (availableArea, index) {
                        if (availableArea.code == area.code) {
                            this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type][index].isSelected = false;
                        }
                    }.bind(this));
                }
                this.$root.updateSelectedAreasForSingleOrderLine(orderLineUuid);
            }

            this.$root.validateAreas();

        },
        isMasterSelected: function (area) {
            var isMasterSelected = false;
            if (this.selected !== undefined && this.selected.length) {
                this.selected.forEach(function (selectedArea) {
                    if (area.code == selectedArea.code) {
                        isMasterSelected = true;
                    }
                });
            }
            return isMasterSelected;
        }
    },
    components: {
        'mapSelect': MapSelect
    }
};

var Projections = {
    props: ['available', 'selected', 'master'],
    template: '#projections-template',

    methods: {
        selectProjection: function (projection) {
            if (this.master) {
                for (orderLineUuid in this.$root.masterOrderLine.allAvailableProjections) {
                    if (this.$root.masterOrderLine.allAvailableProjections[orderLineUuid].length) {
                        this.$root.masterOrderLine.allAvailableProjections[orderLineUuid].forEach(function (availableProjection, index) {
                            if (availableProjection.code == projection.code) {
                                this.$root.masterOrderLine.allAvailableProjections[orderLineUuid][index].isSelected = true;

                                var isAllreadyAddedInfo = this.$root.isAllreadyAdded(this.$parent.selectedProjections, projection, "code");
                                if (!isAllreadyAddedInfo.added) {
                                    this.$parent.selectedProjections.push(projection);
                                }
                            }
                        }.bind(this));
                    }
                }
                this.$root.updateSelectedProjectionsForAllOrderLines();

            } else {
                var orderLineUuid = this.$parent.metadata.uuid;
                if (this.$root.masterOrderLine.allAvailableProjections[orderLineUuid] !== undefined && this.$root.masterOrderLine.allAvailableProjections[orderLineUuid].length) {
                    this.$root.masterOrderLine.allAvailableProjections[orderLineUuid].forEach(function (availableProjection, index) {
                        if (availableProjection.code == projection.code) {
                            this.$root.masterOrderLine.allAvailableProjections[orderLineUuid][index].isSelected = true;
                        }
                    }.bind(this));
                }
                this.$root.updateSelectedProjectionsForSingleOrderLine(orderLineUuid);
            }
            this.$root.validateAreas();



        },
        removeSelectedProjection: function (projection) {
            if (this.master) {
                for (orderLineUuid in this.$root.masterOrderLine.allAvailableProjections) {
                    if (this.$root.masterOrderLine.allAvailableProjections[orderLineUuid].length) {
                        this.$root.masterOrderLine.allAvailableProjections[orderLineUuid].forEach(function (availableProjection, index) {
                            if (availableProjection.code == projection.code) {
                                this.$root.masterOrderLine.allAvailableProjections[orderLineUuid][index].isSelected = false;
                                this.$parent.selectedProjections.forEach(function (selectedProjection, index) {
                                    if (selectedProjection.code == projection.code) {
                                        this.$parent.selectedProjections.splice(index, 1)
                                    }
                                }.bind(this));
                            }
                        }.bind(this));
                    }
                }
                this.$root.updateSelectedProjectionsForAllOrderLines();

            } else {
                var orderLineUuid = this.$parent.metadata.uuid;
                if (this.$root.masterOrderLine.allAvailableProjections[orderLineUuid] !== undefined && this.$root.masterOrderLine.allAvailableProjections[orderLineUuid].length) {
                    this.$root.masterOrderLine.allAvailableProjections[orderLineUuid].forEach(function (availableProjection, index) {
                        if (availableProjection.code == projection.code) {
                            this.$root.masterOrderLine.allAvailableProjections[orderLineUuid][index].isSelected = false;
                        }
                    }.bind(this));
                }
                this.$root.updateSelectedProjectionsForSingleOrderLine(orderLineUuid);
            }
            this.$root.validateAreas();
        },
        isMasterSelected: function (projection) {
            var isMasterSelected = false;
            if (this.selected !== undefined && this.selected.length) {
                this.selected.forEach(function (selectedProjection) {
                    if (projection.code == selectedProjection.code) {
                        isMasterSelected = true;
                    }
                });
            }
            return isMasterSelected;
        }
    }
};

var Formats = {
    props: ['available', 'selected', 'master'],
    template: '#formats-template',

    methods: {
        selectFormat: function (format) {
            if (this.master) {
                for (orderLineUuid in this.$root.masterOrderLine.allAvailableFormats) {
                    if (this.$root.masterOrderLine.allAvailableFormats[orderLineUuid].length) {
                        this.$root.masterOrderLine.allAvailableFormats[orderLineUuid].forEach(function (availableFormat, index) {
                            if (availableFormat.name == format.name) {
                                this.$root.masterOrderLine.allAvailableFormats[orderLineUuid][index].isSelected = true;
                                var isAllreadyAddedInfo = this.$root.isAllreadyAdded(this.$parent.selectedFormats, format, "name");
                                if (!isAllreadyAddedInfo.added) {
                                    this.$parent.selectedFormats.push(format);
                                }
                            }
                        }.bind(this));
                    }
                }
                this.$root.updateSelectedFormatsForAllOrderLines();

            } else {
                var orderLineUuid = this.$parent.metadata.uuid;
                if (this.$root.masterOrderLine.allAvailableFormats[orderLineUuid] !== undefined && this.$root.masterOrderLine.allAvailableFormats[orderLineUuid].length) {
                    this.$root.masterOrderLine.allAvailableFormats[orderLineUuid].forEach(function (availableFormat, index) {
                        if (availableFormat.name == format.name) {
                            this.$root.masterOrderLine.allAvailableFormats[orderLineUuid][index].isSelected = true;
                        }
                    }.bind(this));
                }
                this.$root.updateSelectedFormatsForSingleOrderLine(orderLineUuid);
            }
            this.$root.validateAreas();
        },
        removeSelectedFormat: function (format) {
            if (this.master) {
                for (orderLineUuid in this.$root.masterOrderLine.allAvailableFormats) {
                    if (this.$root.masterOrderLine.allAvailableFormats[orderLineUuid].length) {
                        this.$root.masterOrderLine.allAvailableFormats[orderLineUuid].forEach(function (availableFormat, index) {
                            if (availableFormat.name == format.name) {
                                this.$root.masterOrderLine.allAvailableFormats[orderLineUuid][index].isSelected = false;
                                this.$parent.selectedFormats.forEach(function (selectedFormat, index) {
                                    if (selectedFormat.name == format.name) {
                                        this.$parent.selectedFormats.splice(index, 1)
                                    }
                                }.bind(this));
                            }
                        }.bind(this));
                    }
                }
                this.$root.updateSelectedFormatsForAllOrderLines();

            } else {
                var orderLineUuid = this.$parent.metadata.uuid;
                if (this.$root.masterOrderLine.allAvailableFormats[orderLineUuid] !== undefined && this.$root.masterOrderLine.allAvailableFormats[orderLineUuid].length) {
                    this.$root.masterOrderLine.allAvailableFormats[orderLineUuid].forEach(function (availableFormat, index) {
                        if (availableFormat.name == format.name) {
                            this.$root.masterOrderLine.allAvailableFormats[orderLineUuid][index].isSelected = false;
                        }
                    }.bind(this));
                }
                this.$root.updateSelectedFormatsForSingleOrderLine(orderLineUuid);
            }
            this.$root.validateAreas();
        },
        isMasterSelected: function (format) {
            var isMasterSelected = false;
            if (this.selected !== undefined && this.selected.length) {
                this.selected.forEach(function (selectedFormat) {
                    if (format.name == selectedFormat.name) {
                        isMasterSelected = true;
                    }
                });
            }
            return isMasterSelected;
        }
    }
};



var OrderLine = {
    props: ['metadata', 'capabilities', 'availableAreas', 'availableProjections', 'availableFormats', 'selectedAreas', 'selectedProjections', 'selectedFormats', 'selectedCoordinates', 'defaultProjections', 'defaultFormats', 'orderLineErrors'],
    template: '#order-line-template',
    data: function () {
        var data = {
            expanded: false,
            mapData: {},
            mapIsLoaded: false,
            showMap: false,
            master: false
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
        isAllreadyAdded: function (array, item, propertyToCompare) {
            var isAllreadyAdded = {
                added: false,
                position: 0
            };
            if (array.length) {
                array.forEach(function (arrayItem, index) {
                    if (this.readProperty(arrayItem, propertyToCompare) == this.readProperty(item, propertyToCompare)) {
                        isAllreadyAdded.added = true
                        isAllreadyAdded.position = index;
                    };
                }.bind(this))
            }
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
                var hasResults = false;
                for (var i = 0; i < listItems.length; i++) {
                    if (listItems[i].innerHTML.toUpperCase().indexOf(filter) > -1) {
                        listItems[i].style.display = "";
                        hasResults = true;
                    } else {
                        listItems[i].style.display = "none";
                    }
                }

                var optionGroupNameElement = $(dropdownListElements[listIndex]).closest("div").find(".custom-select-list-option-group-name");
                if (!hasResults) {
                    optionGroupNameElement.hide();
                } else {
                    optionGroupNameElement.show();
                }
            }
        },
        updateSelectedAreas: function () {
            /* var orderLineUuid = this.metadata.uuid;
             var selectedAreas = [];
             for (areaType in this.$parent.masterOrderLine.allAvailableAreas[orderLineUuid]) {
                 if (this.$parent.masterOrderLine.allAvailableAreas[orderLineUuid][areaType].length) {
                     this.$parent.masterOrderLine.allAvailableAreas[orderLineUuid][areaType].forEach(function (localSelectedArea) {
                         if (localSelectedArea.isLocalSelected) {
                             var isAllreadyAddedInfo = this.isAllreadyAdded(selectedAreas, localSelectedArea, "code");
                             selectedAreas.push(localSelectedArea);
                         }
                     }.bind(this))
                 }
             }
             this.$parent.masterOrderLine.allSelectedAreas[orderLineUuid] = selectedAreas;*/
        },
        autoSelectProjectionAndFormats: function () {
            /*  var orderLineUuid = this.metadata.uuid;
              this.$root.masterOrderLine.allSelectedAreas[orderLineUuid].forEach(function (selectedArea) {
                  if (selectedArea.allAvailableProjections.length == 1) {
                      var projectionCode = selectedArea.allAvailableProjections[0].code;
                      this.$root.masterOrderLine.allAvailableProjections[orderLineUuid].forEach(function (availableProjection) {
                          if (availableProjection.code == projectionCode) {
                              availableProjection.isLocalSelected = true;
                          }
                      })
                  }
                  if (selectedArea.allAvailableFormats.length == 1) {
                      var formatName = selectedArea.allAvailableFormats[0].name;
                      this.$root.masterOrderLine.allAvailableFormats[orderLineUuid].forEach(function (availableFormat) {
                          if (availableFormat.name == formatName) {
                              availableFormat.isLocalSelected = true;
                          }
                      })
                  }
              });*/
        },
        updateAvailableProjections: function () {
            /* var orderLineUuid = this.metadata.uuid;
             var availableProjections = [];
             if (this.$parent.masterOrderLine.allSelectedAreas[orderLineUuid].length) {
                 this.$parent.masterOrderLine.allSelectedAreas[orderLineUuid].forEach(function (selectedArea) {
                     if (selectedArea.allAvailableProjections[orderLineUuid] !== undefined && selectedArea.allAvailableProjections[orderLineUuid].length) {
                         selectedArea.allAvailableProjections[orderLineUuid].forEach(function (availableProjection) {
                             var isAllreadyAddedInfo = this.isAllreadyAdded(availableProjections, availableProjection, "code");
                             if (!isAllreadyAddedInfo.added) {
                                 availableProjections.push(availableProjection);
                             }
                         }.bind(this))
                     }
 
                 }.bind(this))
             }
             this.$parent.masterOrderLine.allAvailableProjections[orderLineUuid] = availableProjections;*/
        },
        updateSelectedProjections: function () {
            /* var orderLineUuid = this.metadata.uuid;
             var selectedProjections = [];
             if (this.$parent.masterOrderLine.allAvailableProjections[orderLineUuid] !== undefined && this.$parent.masterOrderLine.allAvailableProjections[orderLineUuid].length) {
                 this.$parent.masterOrderLine.allAvailableProjections[orderLineUuid].forEach(function (localSelectedProjection) {
                     if (localSelectedProjection.isLocalSelected) {
                         var isAllreadyAddedInfo = this.isAllreadyAdded(selectedProjections, localSelectedProjection, "code");
                         if (!isAllreadyAddedInfo.added) {
                             selectedProjections.push(localSelectedProjection);
                         }
                     }
                 }.bind(this))
             }
 
             this.$parent.masterOrderLine.allSelectedProjections[orderLineUuid] = selectedProjections;*/
        },
        updateAvailableFormats: function () {
            /* var orderLineUuid = this.metadata.uuid;
             var availableFormats = [];
             if (this.$parent.masterOrderLine.allSelectedAreas[orderLineUuid].length) {
                 this.$parent.masterOrderLine.allSelectedAreas[orderLineUuid].forEach(function (selectedArea) {
                     if (selectedArea.allAvailableFormats[orderLineUuid] !== undefined && selectedArea.allAvailableFormats[orderLineUuid].length) {
                         selectedArea.allAvailableFormats[orderLineUuid].forEach(function (availableFormat) {
                             var isAllreadyAddedInfo = this.isAllreadyAdded(availableFormats, availableFormat, "name");
                             if (!isAllreadyAddedInfo.added) {
                                 availableFormats.push(availableFormat);
                             }
                         }.bind(this))
                     }
 
                 }.bind(this))
             }
             this.$parent.masterOrderLine.allAvailableFormats[orderLineUuid] = availableFormats;*/
        },
        updateSelectedFormats: function () {
            /* var orderLineUuid = this.metadata.uuid;
             var selectedFormats = [];
             if (this.$parent.masterOrderLine.allAvailableFormats[orderLineUuid] !== undefined && this.$parent.masterOrderLine.allAvailableFormats[orderLineUuid].length) {
                 this.$parent.masterOrderLine.allAvailableFormats[orderLineUuid].forEach(function (localSelectedFormat) {
                     if (localSelectedFormat.isLocalSelected) {
                         var isAllreadyAddedInfo = this.isAllreadyAdded(selectedFormats, localSelectedFormat, "name");
                         if (!isAllreadyAddedInfo.added) {
                             selectedFormats.push(localSelectedFormat);
                         }
                     }
                 }.bind(this))
             }
 
             this.$parent.masterOrderLine.allSelectedFormats[orderLineUuid] = selectedFormats;*/
        },
        selectFromMap: function (orderItem, mapType) {
            orderItem.showMap = true;
            var fixed = orderItem.capabilities.supportsGridSelection;
            if (mapType == "grid") { this.loadGridMap(orderItem) }
            else if (mapType == "polygon") { this.loadPolygonMap(orderItem) }
            $('#norgeskartmodal #setcoordinates').attr('uuid', orderItem.metadata.uuid);
        },

        isJson: function (str) {
            try {
                JSON.parse(str);
            } catch (e) {
                return false;
            }
            return true;
        },

        loadGridMap: function (orderItem) {
            orderItem.mapIsLoaded = true;
            orderItem.mapData.defaultConfigurations = {
                "service_name": orderItem.capabilities.mapSelectionLayer,
                "center_longitude": "378604",
                "center_latitude": "7226208",
                "zoom_level": "3"
            }

            window.addEventListener('message', function (e) {
                if (e !== undefined && e.data !== undefined && typeof (e.data) == "string") {
                    var msg = JSON.parse(e.data);
                    if (msg.type === "mapInitialized") {

                        var iframeElement = document.getElementById(orderItem.metadata.uuid + "-iframe").contentWindow;

                        iframeMessage = {
                            "cmd": "setCenter",
                            "x": orderItem.mapData.defaultConfigurations.center_longitude,
                            "y": orderItem.mapData.defaultConfigurations.center_latitude,
                            "zoom": orderItem.mapData.defaultConfigurations.zoom_level
                        };
                        iframeElement.postMessage(JSON.stringify(iframeMessage), '*');

                        iframeMessage = {
                            "cmd": "setVisible",
                            "id": orderItem.mapData.defaultConfigurations.service_name
                        };
                        iframeElement.postMessage(JSON.stringify(iframeMessage), '*');

                    } else {
                        if (msg.cmd === "setVisible") return;
                        var obj = msg;

                        if (this.isJson(msg)) {
                            var data = JSON.parse(msg);
                            if (data["type"] == "mapInitialized") return;

                            var areaname = data["attributes"]["n"];



                            if (data["cmd"] == "featureSelected") {
                                for (areaType in this.$root.masterOrderLine.allAvailableAreas[orderItem.metadata.uuid]) {
                                    this.$root.masterOrderLine.allAvailableAreas[orderItem.metadata.uuid][areaType].forEach(function (availableArea) {
                                        if (availableArea.code == areaname) {
                                            availableArea.isLocalSelected = true;
                                        }
                                    })
                                }
                            }
                            if (data["cmd"] == "featureUnselected") {
                                for (areaType in this.$root.masterOrderLine.allAvailableAreas[orderItem.metadata.uuid]) {
                                    this.$root.masterOrderLine.allAvailableAreas[orderItem.metadata.uuid][areaType].forEach(function (availableArea) {
                                        if (availableArea.code == areaname) {
                                            availableArea.isLocalSelected = false;
                                        }
                                    })
                                }
                            }
                            this.updateSelectedAreas();
                            this.updateAvailableProjections();
                            this.updateAvailableFormats();
                            this.$root.validateAreas();
                        }
                    }
                }
            }.bind(this));
        },
        loadPolygonMap: function (orderItem) {
            var coverageParams = "";
            $.ajax({
                url: '/api/getdata/' + orderItem.metadata.uuid,
                type: "GET",
                async: false,
                success: function (result) {
                    coverageParams = result.CoverageUrl;
                    if (typeof coverageParams != 'undefined') {
                        orderItem.mapData.coverageParams = coverageParams;
                    }
                }
            });
            orderItem.mapIsLoaded = true;
            orderItem.mapData.defaultConfigurations = {
                center_latitude: "7226208",
                center_longitude: "378604",
                grid_folder: "/sites/all/modules/custom/kms_widget/grid/",
                coordinateSystem: "32633",
                selection_type: "3525",
                service_name: "fylker-utm32",
                zoom_level: "4",
            }

            window.addEventListener('message', function (e) {
                if (e !== undefined && e.data !== undefined && typeof (e.data) == "string") {
                    var msg = JSON.parse(e.data);
                    if (msg.type === "mapInitialized") {
                        iframeMessage = {
                            "cmd": "setCenter",
                            "x": orderItem.mapData.defaultConfigurations.center_longitude,
                            "y": orderItem.mapData.defaultConfigurations.center_latitude,
                            "zoom": orderItem.mapData.defaultConfigurations.zoom_level
                        };
                        var iframeElement = document.getElementById(orderItem.metadata.uuid + "-iframe").contentWindow;
                        iframeElement.postMessage(JSON.stringify(iframeMessage), '*');
                    }
                    else if (msg.cmd === "setVisible") { return }
                    else {
                        var reslist = document.getElementById('result');
                        if (msg.feature != null) {

                            var coordinatesString = msg.feature.geometry.coordinates.toString();
                            coordinatesString = coordinatesString.replace(/,/g, " ");
                            var canDownloadData = {
                                "metadataUuid": orderItem.metadata.uuid,
                                "coordinates": coordinatesString,
                                "coordinateSystem": orderItem.mapData.defaultConfigurations.coordinateSystem
                            };
                            var urlCanDownload = (orderItem.metadata.canDownloadUrl !== undefined) ? orderItem.metadata.canDownloadUrl : false;
                            if (urlCanDownload) {
                                $.ajax({
                                    url: urlCanDownload,
                                    type: "POST",
                                    dataType: 'json',
                                    data: JSON.stringify(canDownloadData),
                                    contentType: "application/json",
                                    async: true,
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        //Ignore error
                                    },
                                    beforeSend: function () {
                                        showLoadingAnimation("Sjekker størrelse for valgt område");
                                    },
                                    success: function (data) {
                                        if (data !== null) {
                                            if (!data.canDownload) {
                                                clearAlertMessage();
                                                showAlert("Området er for stort til å laste ned, vennligst velg mindre område", "danger");
                                            } else {
                                                clearAlertMessage();
                                                hideAlert();

                                                this.$root.masterOrderLine.allSelectedCoordinates[orderItem.metadata.uuid] = coordinatesString;
                                                var polygonArea = {
                                                    "name": "Valgt fra kart",
                                                    "type": "polygon",
                                                    "code": "Kart",
                                                    "isLocalSelected": true,
                                                    "formats": orderItem.defaultFormats,
                                                    "projections": orderItem.defaultProjections,
                                                    "coordinates": coordinatesString,
                                                    "coordinatesystem": orderItem.mapData.defaultConfigurations.coordinateSystem
                                                }

                                                polygonArea.allAvailableProjections = {};
                                                polygonArea.allAvailableProjections[orderItem.metadata.uuid] = orderItem.defaultProjections;

                                                polygonArea.allAvailableFormats = {};
                                                polygonArea.allAvailableFormats[orderItem.metadata.uuid] = orderItem.defaultFormats;

                                                var isAllreadyAddedInfo = this.isAllreadyAdded(this.$root.masterOrderLine.allSelectedAreas[orderItem.metadata.uuid], polygonArea, "code");
                                                if (!isAllreadyAddedInfo.added) {
                                                    this.$root.masterOrderLine.allSelectedAreas[orderItem.metadata.uuid].push(polygonArea);
                                                } else {
                                                    this.$root.masterOrderLine.allSelectedAreas[orderItem.metadata.uuid][isAllreadyAddedInfo.position] = polygonArea;

                                                }

                                                this.$root.masterOrderLine.allAvailableAreas[orderItem.metadata.uuid][polygonArea.type] = [];
                                                this.$root.masterOrderLine.allAvailableAreas[orderItem.metadata.uuid][polygonArea.type].push(polygonArea);


                                                // Set coordinates for orderline in order request
                                                this.$root.orderRequests[orderItem.metadata.orderDistributionUrl].orderLines.forEach(function (orderRequest) {
                                                    if (orderRequest.metadataUuid == orderItem.metadata.uuid) {
                                                        orderRequest.coordinates = this.$root.masterOrderLine.allSelectedCoordinates[orderItem.metadata.uuid];
                                                    }
                                                }.bind(this))

                                                this.$root.$forceUpdate();

                                                this.updateAvailableProjections();
                                                this.updateAvailableFormats();
                                                this.$root.validateAreas();
                                            }
                                        }
                                        hideLoadingAnimation();
                                    }.bind(this),
                                });
                            }
                        }
                    }
                }
            }.bind(this));

        }
    },
    components: {
        'areas': Areas,
        'projections': Projections,
        'formats': Formats
    },
};

var MasterOrderLine = {
    props: ['allAvailableAreas', 'availableProjections', 'availableFormats', 'allSelectedAreas', 'allSelectedProjections', 'allSelectedFormats', 'allOrderLineErrors'],
    data: function () {
        var data = {
            availableAreas: {},

            selectedAreas: [],
            selectedProjections: [],
            selectedFormats: [],

            mapData: {},
            mapIsLoaded: false,
            showMap: false,

            master: true
        }
        return data;
    },

    created: function () {
        for (orderLine in this.allAvailableAreas) {

            if (this.$parent.masterOrderLine.allSelectedAreas[orderLine] == undefined) { this.$parent.masterOrderLine.allSelectedAreas[orderLine] = [] }
            if (this.$parent.masterOrderLine.allSelectedProjections[orderLine] == undefined) { this.$parent.masterOrderLine.allSelectedProjections[orderLine] = [] }
            if (this.$parent.masterOrderLine.allSelectedFormats[orderLine] == undefined) { this.$parent.masterOrderLine.allSelectedFormats[orderLine] = [] }

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
                            this.availableAreas[areaType][areaIsAllreadyAddedInfo.position].orderLineUuids.push(orderLine);
                            this.availableAreas[areaType][areaIsAllreadyAddedInfo.position].allAvailableFormats[orderLine] = area.formats;
                        }
                    }
                }.bind(this))
            }
        }
        this.$root.validateAreas();
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
                var hasResults = false;
                for (var i = 0; i < listItems.length; i++) {
                    if (listItems[i].innerHTML.toUpperCase().indexOf(filter) > -1) {
                        listItems[i].style.display = "";
                        hasResults = true;
                    } else {
                        listItems[i].style.display = "none";
                    }
                }

                var optionGroupNameElement = $(dropdownListElements[listIndex]).closest("div").find(".custom-select-list-option-group-name");
                if (!hasResults) {
                    optionGroupNameElement.hide();
                } else {
                    optionGroupNameElement.show();
                }
            }
        },
        updateSelectedAreas: function () {
            /* var allSelectedAreas = {};
             var selectedAreas = [];
             for (areaType in this.availableAreas) {
                 this.availableAreas[areaType].forEach(function (area) {
                     if (area.orderLineUuids !== undefined) {
 
                         area.orderLineUuids.forEach(function (orderLineUuid) {
                             if (allSelectedAreas[orderLineUuid] == undefined) { allSelectedAreas[orderLineUuid] = [] }
                             if (area.isSelected || area.isLocalSelected) {
                                 allSelectedAreas[orderLineUuid].push(area);
                             }
                         }.bind(this))
                     }
                     if (area.isSelected) {
                         var isAllreadyAddedInfo = this.isAllreadyAdded(selectedAreas, area, "code");
                         if (!isAllreadyAddedInfo.added) {
                             selectedAreas.push(area);
                         }
                     }
                 }.bind(this));
             }
             this.$parent.masterOrderLine.allSelectedAreas = allSelectedAreas;
             this.selectedAreas = selectedAreas;*/

        },
        updateAvailableProjections: function () {
            /* var availableProjections = [];
             var allAvailableProjections = {}
             if (this.$parent.masterOrderLine.allSelectedAreas !== undefined) {
                 for (orderLine in this.$parent.masterOrderLine.allSelectedAreas) {
 
                     this.$parent.masterOrderLine.allSelectedAreas[orderLine].forEach(function (selectedArea) {
                         if (selectedArea.allAvailableProjections !== undefined && selectedArea.allAvailableProjections[orderLine] !== undefined && selectedArea.allAvailableProjections[orderLine].length) {
 
                             // All available projections for orderLine
                             selectedArea.allAvailableProjections[orderLine].forEach(function (projection) {
                                 if (projection.isSelected == undefined) { projection.isSelected = false }
                                 if (projection.isLocalSelected == undefined) { projection.isLocalSelected = false }
 
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
             */
        },
        updateAvailableFormats: function () {
            /* var availableFormats = [];
             var allAvailableFormats = {};
             if (this.$parent.masterOrderLine.allSelectedAreas !== undefined) {
                 for (orderLine in this.$parent.masterOrderLine.allSelectedAreas) {
 
                     this.$parent.masterOrderLine.allSelectedAreas[orderLine].forEach(function (selectedArea) {
                         if (selectedArea.allAvailableFormats !== undefined && selectedArea.allAvailableFormats[orderLine] !== undefined && selectedArea.allAvailableFormats[orderLine].length) {
 
                             // All available formats for orderLine
                             selectedArea.allAvailableFormats[orderLine].forEach(function (format) {
                                 if (format.isSelected == undefined) { format.isSelected = false }
                                 if (format.isLocalSelected == undefined) { format.isLocalSelected = false }
 
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
             this.$parent.masterOrderLine.allAvailableFormats = allAvailableFormats;*/
        },

        updateSelectedProjections: function () {
            /*   var allSelectedProjections = {};
               var selectedProjections = [];
               for (orderLine in this.$parent.masterOrderLine.allAvailableProjections) {
                   this.$parent.masterOrderLine.allAvailableProjections[orderLine].forEach(function (availableProjection) {
   
                       if (availableProjection.isSelected || availableProjection.isLocalSelected) {
   
                           if (availableProjection.isSelected) {
                               // Update availableProjections array
                               var isAllreadyAddedInfo = this.isAllreadyAdded(selectedProjections, availableProjection, "code");
                               if (!isAllreadyAddedInfo.added) {
                                   selectedProjections.push(availableProjection);
                               }
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
               this.selectedProjections = selectedProjections;*/
        },

        updateSelectedFormats: function () {
            /*  var allSelectedFormats = {};
              var selectedFormats = [];
              for (orderLine in this.$parent.masterOrderLine.allAvailableFormats) {
                  this.$parent.masterOrderLine.allAvailableFormats[orderLine].forEach(function (availableFormats) {
  
                      if (availableFormats.isSelected || availableFormats.isLocalSelected) {
  
                          if (availableFormats.isSelected) {
                              // Update availableFormats array
                              var isAllreadyAddedInfo = this.isAllreadyAdded(selectedFormats, availableFormats, "name");
                              if (!isAllreadyAddedInfo.added) {
                                  selectedFormats.push(availableFormats);
                              }
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
              this.selectedFormats = selectedFormats;*/
        },
        masterSupportsPolygonSelection: function () {
            var masterSupportsPolygonSelection = false;
            this.$root.orderLines.forEach(function (orderLine) {
                if (orderLine.capabilities.supportsPolygonSelection)
                    masterSupportsPolygonSelection = true;
            });
            return masterSupportsPolygonSelection;
        },
        getFirstOrderItemWithPolygonSupport: function () {
            var firstOrderItemWithPolygonSupport = false;
            this.$root.orderLines.forEach(function (orderLine) {
                if (orderLine.capabilities.supportsPolygonSelection) {
                    firstOrderItemWithPolygonSupport = orderLine;
                    return;
                }
            });
            return firstOrderItemWithPolygonSupport;
        },
        selectFromMap: function (orderItem, mapType) {
            orderItem.showMap = true;
            var firstOrderItemWithPolygonSupport = this.getFirstOrderItemWithPolygonSupport();
            this.loadPolygonMap(firstOrderItemWithPolygonSupport);
            //$('#norgeskartmodal #setcoordinates').attr('uuid', orderItem.metadata.uuid);
        },
        loadPolygonMap: function (firstOrderItemWithPolygonSupport) {
            var coverageParams = "";
            $.ajax({
                url: '/api/getdata/' + firstOrderItemWithPolygonSupport.metadata.uuid,
                type: "GET",
                async: false,
                success: function (result) {
                    coverageParams = result.CoverageUrl;
                    if (typeof coverageParams != 'undefined') {
                        this.mapData.coverageParams = coverageParams;
                    }
                }.bind(this)
            });
            this.mapIsLoaded = true;
            this.mapData.defaultConfigurations = {
                center_latitude: "7226208",
                center_longitude: "378604",
                grid_folder: "/sites/all/modules/custom/kms_widget/grid/",
                coordinateSystem: "32633",
                selection_type: "3525",
                service_name: "fylker-utm32",
                zoom_level: "4",
            }

            window.addEventListener('message', function (e) {
                if (e !== undefined && e.data !== undefined && typeof (e.data) == "string") {
                    var msg = JSON.parse(e.data);
                    if (msg.type === "mapInitialized") {
                        iframeMessage = {
                            "cmd": "setCenter",
                            "x": this.mapData.defaultConfigurations.center_longitude,
                            "y": this.mapData.defaultConfigurations.center_latitude,
                            "zoom": this.mapData.defaultConfigurations.zoom_level
                        };
                        var iframeElement = document.getElementById("masterorderline-iframe").contentWindow;
                        iframeElement.postMessage(JSON.stringify(iframeMessage), '*');
                    }
                    else if (msg.cmd === "setVisible") { return }
                    else {
                        var reslist = document.getElementById('result');
                        if (msg.feature != null) {

                            var coordinatesString = msg.feature.geometry.coordinates.toString();
                            coordinatesString = coordinatesString.replace(/,/g, " ");
                            var canDownloadData = {
                                "metadataUuid": firstOrderItemWithPolygonSupport.metadata.uuid,
                                "coordinates": coordinatesString,
                                "coordinateSystem": this.mapData.defaultConfigurations.coordinateSystem
                            };
                            var urlCanDownload = (firstOrderItemWithPolygonSupport.metadata.canDownloadUrl !== undefined) ? firstOrderItemWithPolygonSupport.metadata.canDownloadUrl : false;
                            if (urlCanDownload) {
                                $.ajax({
                                    url: urlCanDownload,
                                    type: "POST",
                                    dataType: 'json',
                                    data: JSON.stringify(canDownloadData),
                                    contentType: "application/json",
                                    async: true,
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        //Ignore error
                                    },
                                    beforeSend: function () {
                                        showLoadingAnimation("Sjekker størrelse for valgt område");
                                    },
                                    success: function (data) {
                                        if (data !== null) {
                                            if (!data.canDownload) {
                                                clearAlertMessage();
                                                showAlert("Området er for stort til å laste ned, vennligst velg mindre område", "danger");
                                            } else {
                                                clearAlertMessage();
                                                hideAlert();

                                                mainVueModel.$children.forEach(function (orderItem) {
                                                    if (orderItem.master !== undefined && orderItem.master == false) {
                                                        if (orderItem.capabilities !== undefined && orderItem.capabilities.supportsPolygonSelection !== undefined && orderItem.capabilities.supportsPolygonSelection == true) {

                                                            this.$root.masterOrderLine.allSelectedCoordinates[orderItem.metadata.uuid] = coordinatesString;
                                                            var polygonArea = {
                                                                "name": "Valgt fra kart",
                                                                "type": "polygon",
                                                                "code": "Kart",
                                                                "isSelected": true,
                                                                "isLocalSelected": true,
                                                                "formats": orderItem.defaultFormats,
                                                                "projections": orderItem.defaultProjections,
                                                                "coordinates": coordinatesString,
                                                                "coordinatesystem": this.mapData.defaultConfigurations.coordinateSystem
                                                            }

                                                            polygonArea.allAvailableProjections = {};
                                                            polygonArea.allAvailableProjections[orderItem.metadata.uuid] = orderItem.defaultProjections;

                                                            polygonArea.allAvailableFormats = {};
                                                            polygonArea.allAvailableFormats[orderItem.metadata.uuid] = orderItem.defaultFormats;


                                                            // orderLine
                                                            var isAllreadyAddedInfo = this.isAllreadyAdded(this.$root.masterOrderLine.allSelectedAreas[orderItem.metadata.uuid], polygonArea, "code");
                                                            if (!isAllreadyAddedInfo.added) {
                                                                this.$root.masterOrderLine.allSelectedAreas[orderItem.metadata.uuid].push(polygonArea);
                                                            } else {
                                                                this.$root.masterOrderLine.allSelectedAreas[orderItem.metadata.uuid][isAllreadyAddedInfo.position] = polygonArea;
                                                            }

                                                            this.$root.masterOrderLine.allAvailableAreas[orderItem.metadata.uuid][polygonArea.type] = [];
                                                            this.$root.masterOrderLine.allAvailableAreas[orderItem.metadata.uuid][polygonArea.type].push(polygonArea);


                                                            // MasterOrderLine:
                                                            var isAllreadyAddedInfo = this.isAllreadyAdded(this.selectedAreas, polygonArea, "code");
                                                            if (!isAllreadyAddedInfo.added) {
                                                                this.selectedAreas.push(polygonArea);
                                                            } else {
                                                                this.selectedAreas[isAllreadyAddedInfo.position] = polygonArea;
                                                            }

                                                            this.availableAreas[polygonArea.type] = [];
                                                            this.availableAreas[polygonArea.type].push(polygonArea);


                                                            // Set coordinates for orderline in order request
                                                            this.$root.orderRequests[orderItem.metadata.orderDistributionUrl].orderLines.forEach(function (orderRequest) {
                                                                if (orderRequest.metadataUuid == orderItem.metadata.uuid) {
                                                                    orderRequest.coordinates = this.$root.masterOrderLine.allSelectedCoordinates[orderItem.metadata.uuid];
                                                                }
                                                            }.bind(this))
                                                        }
                                                    }
                                                }.bind(this));

                                                this.$root.$forceUpdate();

                                                this.updateAvailableProjections();
                                                this.updateAvailableFormats();
                                                this.$root.validateAreas();
                                            }
                                        }
                                        hideLoadingAnimation();
                                    }.bind(this),
                                });
                            }
                        }
                    }
                }
            }.bind(this));

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

        masterOrderLine: {
            allAvailableAreas: {},
            allAvailableProjections: {},
            masterAvailableProjections: [],
            allAvailableFormats: {},
            masterAvailableFormats: [],
            allSelectedAreas: {},
            allSelectedProjections: {},
            allSelectedFormats: {},
            allOrderLineErrors: {},
            allSelectedCoordinates: {},
            allDefaultProjections: {},
            allDefaultFormats: {}
        }
    },
    computed: {
        emailRequired: function () {
            var emailRequired = false;
            for (orderLine in this.masterOrderLine.allSelectedAreas) {
                this.masterOrderLine.allSelectedAreas[orderLine].forEach(function (selectedArea) {
                    if (selectedArea.type == "polygon") {
                        emailRequired = true;
                    }
                });
            }
            return emailRequired;
        },
        orderRequests: function () {
            var orderRequests = {};
            if (this.orderLines.length) {
                this.orderLines.forEach(function (orderLine) {

                    if (orderLine.metadata !== undefined) {

                        if (orderRequests[orderLine.metadata.orderDistributionUrl] == undefined) {
                            orderRequests[orderLine.metadata.orderDistributionUrl] = {
                                "email": "",
                                "_links": "",
                                "orderLines": []
                            }
                        }

                        var links = [];
                        if (orderLine.capabilities._links !== undefined && orderLine.capabilities._links.length) {
                            orderLine.capabilities._links.forEach(function (capabilityLink) {
                                var link = {
                                    "href": capabilityLink.href,
                                    "rel": capabilityLink.rel,
                                    "templated": capabilityLink.templatedSpecified,
                                    "type": "",
                                    "deprecation": "",
                                    "name": "",
                                    "title": ""
                                }
                                links.push(link);
                            })
                        }


                        var areas = [];
                        if (this.masterOrderLine.allSelectedAreas[orderLine.metadata.uuid] !== undefined && this.masterOrderLine.allSelectedAreas[orderLine.metadata.uuid].length) {
                            this.masterOrderLine.allSelectedAreas[orderLine.metadata.uuid].forEach(function (selectedArea) {
                                var area = {
                                    "code": selectedArea.code,
                                    "name": selectedArea.name,
                                    "type": selectedArea.type,
                                    "_links": []
                                }
                                areas.push(area);
                            });
                        }

                        var projections = [];
                        if (this.masterOrderLine.allSelectedProjections[orderLine.metadata.uuid] !== undefined && this.masterOrderLine.allSelectedProjections[orderLine.metadata.uuid].length) {
                            this.masterOrderLine.allSelectedProjections[orderLine.metadata.uuid].forEach(function (selectedProjection) {
                                var projection = {
                                    "code": selectedProjection.code,
                                    "name": selectedProjection.name,
                                    "codespace": selectedProjection.codespace,
                                    "_links": []
                                }
                                projections.push(projection);
                            });
                        }

                        var formats = [];
                        if (this.masterOrderLine.allSelectedFormats[orderLine.metadata.uuid] !== undefined && this.masterOrderLine.allSelectedFormats[orderLine.metadata.uuid].length) {
                            this.masterOrderLine.allSelectedFormats[orderLine.metadata.uuid].forEach(function (selectedFormat) {
                                var format = {
                                    "code": "",
                                    "name": selectedFormat.name,
                                    "type": "",
                                    "_links": []
                                }
                                formats.push(format);
                            });
                        }


                        var orderRequest = {
                            "metadataUuid": orderLine.metadata.uuid,
                            "areas": areas,
                            "projections": projections,
                            "formats": formats,
                            "_links": links
                        }

                        if (this.masterOrderLine.allSelectedCoordinates[orderLine.metadata.uuid] !== "") {
                            orderRequest.coordinates = this.masterOrderLine.allSelectedCoordinates[orderLine.metadata.uuid];
                        }

                        orderRequests[orderLine.metadata.orderDistributionUrl].orderLines.push(orderRequest);
                    }
                }.bind(this));
            }
            return orderRequests;
        },
        orderResponseGrouped: function () {
            var orderResponseGrouped = [];
            if (this.orderResponse.length) {
                this.orderResponse.forEach(function (distribution) {
                    var orderResponseGroup = {
                        distributionUrl: distribution.distributionUrl,
                        files: {}
                    }
                    distribution.data.files.forEach(function (file) {
                        if (orderResponseGroup.files[file.metadataName] == undefined) {
                            orderResponseGroup.files[file.metadataName] = [];
                        }
                        orderResponseGroup.files[file.metadataName].push(file);
                    });
                    orderResponseGrouped.push(orderResponseGroup);
                })
            }
            return orderResponseGrouped;
        }
    },
    created: function () {
        $("#vueContainer").removeClass("hidden");
        var defaultUrl = "https://nedlasting.geonorge.no/api/capabilities/";
        var orderItemsJson = (localStorage["orderItems"] != null) ? JSON.parse(localStorage["orderItems"]) : [];
        var orderLines = [];
        if (orderItemsJson.length) {
            var key = 0;
            $(orderItemsJson).each(function (index, val) {
                if (val !== undefined && val !== null && val !== "") {
                    var metadata = (localStorage[val + ".metadata"] !== undefined) ? JSON.parse(localStorage[val + ".metadata"]) : "";
                    var apiUrl = (metadata.distributionUrl !== undefined) ? metadata.distributionUrl : defaultUrl;
                    var capabilities = getJsonData(apiUrl + val);
                    if (capabilities == "error" && metadata !== "") {
                        this.removeOrderLine(metadata.uuid);
                    }
                    else if (capabilities !== "") {
                        orderLines[key] = {
                            "metadata": metadata,
                            "capabilities": capabilities,
                            "projectionAndFormatIsRequired": false
                        }

                        if (metadata !== "" && metadata.uuid !== undefined && metadata.uuid !== "") {

                            var uuid = metadata.uuid;

                            this.masterOrderLine.allAvailableProjections[uuid] = [];
                            this.masterOrderLine.allAvailableFormats[uuid] = [];
                            this.masterOrderLine.allSelectedCoordinates[uuid] = "";
                            this.masterOrderLine.allDefaultProjections[uuid] = [];
                            this.masterOrderLine.allDefaultFormats[uuid] = [];


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
                                            availableArea.isSelected = false;
                                            availableArea.isLocalSelected = false;
                                            this.masterOrderLine.allAvailableAreas[uuid][availableArea.type].push(availableArea);
                                        }.bind(this))
                                    }
                                    if (link.rel == "http://rel.geonorge.no/download/projection") {
                                        var defaultProjections = getJsonData(link.href)
                                        orderLines[key].defaultProjections = defaultProjections;
                                        this.masterOrderLine.allDefaultProjections[uuid] = defaultProjections;
                                    }
                                    if (link.rel == "http://rel.geonorge.no/download/format") {
                                        var defaultFormats = getJsonData(link.href);
                                        orderLines[key].defaultFormats = defaultFormats;
                                        this.masterOrderLine.allDefaultFormats[uuid] = defaultFormats;
                                    }
                                }.bind(this))
                            }
                        }
                        var distributionUrl = (orderLines[key].metadata.distributionUrl !== undefined) ? orderLines[key].metadata.distributionUrl : "";

                        orderLines[key].capabilities.supportsGridSelection = (orderLines[key].capabilities.mapSelectionLayer !== undefined && orderLines[key].capabilities.mapSelectionLayer !== "") ? true : false;
                    }
                    key += 1;
                }
            }.bind(this));
        }
        this.orderLines = orderLines;
    },
    components: {
        'orderLine': OrderLine,
        'masterOrderLine': MasterOrderLine
    },
    methods: {
        isAllreadyAdded: function (array, item, propertyToCompare) {
            var isAllreadyAdded = {
                added: false,
                position: 0
            };
            if (array.length) {
                array.forEach(function (arrayItem, index) {
                    if (this.readProperty(arrayItem, propertyToCompare) == this.readProperty(item, propertyToCompare)) {
                        isAllreadyAdded.added = true
                        isAllreadyAdded.position = index;
                    };
                }.bind(this))
            }
            return isAllreadyAdded;
        },
        readProperty: function (obj, prop) {
            return obj[prop];
        },
        isSupportedType: function (areaType) {
            var isSupportedType = false;
            var supportedAreaTypes = ["fylke", "kommune"];
            supportedAreaTypes.forEach(function (supportedAreaType) {
                if (areaType == supportedAreaType) isSupportedType = true;
            })
            return isSupportedType;
        },
        hasSelectedProjections: function (area, orderLine) {
            var hasSelectedProjections = false;

            if (area.allAvailableProjections !== undefined && area.allAvailableProjections[orderLine] !== undefined && area.allAvailableProjections[orderLine].length) {
                area.allAvailableProjections[orderLine].forEach(function (availableProjection) {
                    if (this.masterOrderLine.allSelectedProjections[orderLine] !== undefined && this.masterOrderLine.allSelectedProjections[orderLine].length) {
                        this.masterOrderLine.allSelectedProjections[orderLine].forEach(function (selectedProjection) {
                            if (selectedProjection.code == availableProjection.code) {
                                hasSelectedProjections = true
                            }
                        }.bind(this))
                    }
                }.bind(this))
            }
            if (!hasSelectedProjections) {
                var errorMessage = "Støttet projeksjon for " + area.name + " mangler";
                this.masterOrderLine.allOrderLineErrors[orderLine]["projection"].push(errorMessage);
            }
            return hasSelectedProjections;
        },
        hasSelectedFormats: function (area, orderLine) {
            var hasSelectedFormats = false;

            if (area.allAvailableFormats !== undefined && area.allAvailableFormats[orderLine] !== undefined && area.allAvailableFormats[orderLine].length) {
                area.allAvailableFormats[orderLine].forEach(function (availableFormat) {
                    if (this.masterOrderLine.allSelectedFormats[orderLine] !== undefined && this.masterOrderLine.allSelectedFormats[orderLine].length) {
                        this.masterOrderLine.allSelectedFormats[orderLine].forEach(function (selectedFormat) {
                            if (selectedFormat.name == availableFormat.name) {
                                hasSelectedFormats = true
                            }
                        }.bind(this))
                    }
                }.bind(this))
            }
            if (!hasSelectedFormats) {
                var errorMessage = "Støttet format for " + area.name + " mangler";
                this.masterOrderLine.allOrderLineErrors[orderLine]["format"].push(errorMessage);
            }
            return hasSelectedFormats;
        },
        validateAreas: function () {
            for (orderLine in this.masterOrderLine.allAvailableAreas) {
                this.masterOrderLine.allOrderLineErrors[orderLine] = {};
                this.masterOrderLine.allOrderLineErrors[orderLine]["projection"] = [];
                this.masterOrderLine.allOrderLineErrors[orderLine]["format"] = [];
                this.masterOrderLine.allOrderLineErrors[orderLine]["area"] = [];
                if (this.masterOrderLine.allSelectedAreas[orderLine] !== undefined && this.masterOrderLine.allSelectedAreas[orderLine].length) {

                    this.masterOrderLine.allSelectedAreas[orderLine].forEach(function (selectedArea) {
                        selectedArea.hasSelectedProjections = this.hasSelectedProjections(selectedArea, orderLine);
                        selectedArea.hasSelectedFormats = this.hasSelectedFormats(selectedArea, orderLine);
                    }.bind(this));

                } else {
                    this.masterOrderLine.allOrderLineErrors[orderLine]["area"] = ["Datasett mangler valgt område"];
                }
            }
            this.$forceUpdate();
            setTimeout(function () {
                $("[data-toggle='tooltip']").tooltip();
            }, 300);
        },


        /* ----- Nye metoder ---- */



        updateAvailableProjectionsAndFormatsForSingleOrderLine: function (orderLineUuid) {
            var availableProjections = [];
            var availableFormats = [];

            if (this.masterOrderLine.allSelectedAreas[orderLineUuid].length) {
                this.masterOrderLine.allSelectedAreas[orderLineUuid].forEach(function (selectedArea) {

                    // Get available projections
                    if (selectedArea.allAvailableProjections[orderLineUuid] !== undefined && selectedArea.allAvailableProjections[orderLineUuid].length) {
                        selectedArea.allAvailableProjections[orderLineUuid].forEach(function (availableProjection) {
                            var isAllreadyAddedInfo = this.isAllreadyAdded(availableProjections, availableProjection, "code");
                            if (!isAllreadyAddedInfo.added) {
                                availableProjections.push(availableProjection);
                            }
                        }.bind(this))
                    }

                    // Get available formats
                    if (selectedArea.allAvailableFormats[orderLineUuid] !== undefined && selectedArea.allAvailableFormats[orderLineUuid].length) {
                        selectedArea.allAvailableFormats[orderLineUuid].forEach(function (availableFormat) {
                            var isAllreadyAddedInfo = this.isAllreadyAdded(availableFormats, availableFormat, "name");
                            if (!isAllreadyAddedInfo.added) {
                                availableFormats.push(availableFormat);
                            }
                        }.bind(this))
                    }

                }.bind(this))
            }
            this.masterOrderLine.allAvailableProjections[orderLineUuid] = availableProjections;
            this.masterOrderLine.allAvailableFormats[orderLineUuid] = availableFormats;
        },

        updateAvailableProjectionsAndFormatsForAllOrderLines: function(){
            this.orderLines.forEach(function (orderLine) {
                if (orderLine.metadata !== undefined && orderLine.metadata.uuid !== undefined) {
                    this.updateAvailableProjectionsAndFormatsForSingleOrderLine(orderLine.metadata.uuid);
                }
            }.bind(this));
        },

        updateAvailableProjectionsAndFormatsForMasterOrderLine: function () {
            var masterAvailableProjections = [];
            var masterAvailableFormats = [];

            if (this.masterOrderLine.allAvailableProjections !== undefined) {
                for (orderLineUuid in this.masterOrderLine.allAvailableProjections) {
                    if (this.masterOrderLine.allAvailableProjections[orderLineUuid].length) {
                        // All available projections for orderLine
                        this.masterOrderLine.allAvailableProjections[orderLineUuid].forEach(function (availableProjection) {

                            // Update availableProjections array
                            var isAllreadyAddedInfo = this.isAllreadyAdded(masterAvailableProjections, availableProjection, "code");
                            if (!isAllreadyAddedInfo.added) {
                                masterAvailableProjections.push(availableProjection);
                            }

                        }.bind(this));
                    }
                }
            }


            if (this.masterOrderLine.allAvailableFormats !== undefined) {
                for (orderLineUuid in this.masterOrderLine.allAvailableFormats) {
                    if (this.masterOrderLine.allAvailableFormats[orderLineUuid].length) {
                        // All available projections for orderLine
                        this.masterOrderLine.allAvailableFormats[orderLineUuid].forEach(function (availableFormat) {

                            // Update availableProjections array
                            var isAllreadyAddedInfo = this.isAllreadyAdded(masterAvailableFormats, availableFormat, "name");
                            if (!isAllreadyAddedInfo.added) {
                                masterAvailableFormats.push(availableFormat);
                            }

                        }.bind(this));
                    }
                }
            }

            this.masterOrderLine.masterAvailableProjections = masterAvailableProjections;
            this.masterOrderLine.masterAvailableFormats = masterAvailableFormats;
        },

        updateSelectedAreasForSingleOrderLine(orderLineUuid) {
            var selectedAreas = [];
            for (areaType in this.masterOrderLine.allAvailableAreas[orderLineUuid]) {
                if (this.masterOrderLine.allAvailableAreas[orderLineUuid][areaType].length) {
                    this.masterOrderLine.allAvailableAreas[orderLineUuid][areaType].forEach(function (selectedArea) {
                        if (selectedArea.isSelected) {
                            var isAllreadyAddedInfo = this.isAllreadyAdded(selectedAreas, selectedArea, "code");
                            if (!isAllreadyAddedInfo.added) {
                                selectedAreas.push(selectedArea);
                            }
                        }
                    }.bind(this))
                }
            }
            this.masterOrderLine.allSelectedAreas[orderLineUuid] = selectedAreas;

            this.updateAvailableProjectionsAndFormatsForSingleOrderLine(orderLineUuid);
            this.autoSelectProjectionAndFormatsForSingleOrderLine(orderLineUuid);
            this.updateSelectedProjectionsForSingleOrderLine(orderLineUuid);
            this.updateSelectedFormatsForSingleOrderLine(orderLineUuid);
            this.updateAvailableProjectionsAndFormatsForMasterOrderLine();
        },


        updateSelectedProjectionsForSingleOrderLine(orderLineUuid) {
            var selectedProjections = [];
            if (this.masterOrderLine.allAvailableProjections[orderLineUuid] !== undefined && this.masterOrderLine.allAvailableProjections[orderLineUuid].length) {
                this.masterOrderLine.allAvailableProjections[orderLineUuid].forEach(function (selectedProjection) {
                    if (selectedProjection.isSelected) {
                        var isAllreadyAddedInfo = this.isAllreadyAdded(selectedProjections, selectedProjection, "code");
                        if (!isAllreadyAddedInfo.added) {
                            selectedProjections.push(selectedProjection);
                        }
                    }
                }.bind(this))
            }
            this.masterOrderLine.allSelectedProjections[orderLineUuid] = selectedProjections;
        },

        updateSelectedFormatsForSingleOrderLine(orderLineUuid) {
            var selectedFormats = [];
            if (this.masterOrderLine.allAvailableFormats[orderLineUuid] !== undefined && this.masterOrderLine.allAvailableFormats[orderLineUuid].length) {
                this.masterOrderLine.allAvailableFormats[orderLineUuid].forEach(function (selectedFormat) {
                    if (selectedFormat.isSelected) {
                        var isAllreadyAddedInfo = this.isAllreadyAdded(selectedFormats, selectedFormat, "name");
                        if (!isAllreadyAddedInfo.added) {
                            selectedFormats.push(selectedFormat);
                        }
                    }
                }.bind(this))
            }
            this.masterOrderLine.allSelectedFormats[orderLineUuid] = selectedFormats;
        },

        updateSelectedAreasForAllOrderLines() {
            this.orderLines.forEach(function (orderLine) {
                if (orderLine.metadata !== undefined && orderLine.metadata.uuid !== undefined) {
                    this.updateSelectedAreasForSingleOrderLine(orderLine.metadata.uuid);
                }
            }.bind(this));
        },

        updateSelectedProjectionsForAllOrderLines() {
            this.orderLines.forEach(function (orderLine) {
                if (orderLine.metadata !== undefined && orderLine.metadata.uuid !== undefined) {
                    this.updateSelectedProjectionsForSingleOrderLine(orderLine.metadata.uuid);
                }
            }.bind(this));
        },

        updateSelectedFormatsForAllOrderLines() {
            this.orderLines.forEach(function (orderLine) {
                if (orderLine.metadata !== undefined && orderLine.metadata.uuid !== undefined) {
                    this.updateSelectedFormatsForSingleOrderLine(orderLine.metadata.uuid);
                }
            }.bind(this));
        },


        /* ---------------------- */

        /*updateSingleOrderLineField: function(orderLineUuid){
            this.updateSelectedProjectionsAndFormats(orderLineUuid);
            this.updateAvailableProjectionsAndFormatsForMasterOrderLine();
        },
        updateAllOrderLineFields: function () {
            this.orderLines.forEach(function (orderLine) {
                if (orderLine.metadata !== undefined && orderLine.metadata.uuid !== undefined) {
                    this.updateSingleOrderLineField(orderLine.metadata.uuid);
                }
            }.bind(this));
            this.$children.forEach(function (orderLine, index) {
                if (orderLine.master !== undefined && !orderLine.master) {
                    orderLine.updateSelectedAreas();
                    orderLine.updateAvailableProjections();
                    orderLine.updateAvailableFormats();
                    orderLine.updateSelectedProjections();
                    orderLine.updateSelectedFormats();
                    orderLine.updateSelectedFormats();
                }
            });
            //this.validateAreas();
       // },*/
        autoSelectProjectionAndFormatsForSingleOrderLine: function (orderLineUuid) {
            this.masterOrderLine.allSelectedAreas[orderLineUuid].forEach(function (selectedArea) {
                if (selectedArea.allAvailableProjections[orderLineUuid].length == 1) {
                    var projectionCode = selectedArea.allAvailableProjections[orderLineUuid][0].code;
                    this.masterOrderLine.allAvailableProjections[orderLineUuid].forEach(function (availableProjection, index) {
                        if (availableProjection.code == projectionCode) {
                            this.masterOrderLine.allAvailableProjections[orderLineUuid][index].isSelected = true;
                        }
                    }.bind(this));
                }
                if (selectedArea.allAvailableFormats[orderLineUuid].length == 1) {
                    var formatName = selectedArea.allAvailableFormats[orderLineUuid][0].name;
                    this.masterOrderLine.allAvailableFormats[orderLineUuid].forEach(function (availableFormat, index) {
                        if (availableFormat.name == formatName) {
                            this.masterOrderLine.allAvailableFormats[orderLineUuid][index].isSelected = true;
                        }
                    }.bind(this));
                }
            }.bind(this));
        },
        sendRequests: function () {
            var responseData = [];
            var responseFailed = false;
            var orderRequests = this.orderRequests;
            for (distributionUrl in orderRequests) {
                $.ajax({
                    url: distributionUrl,
                    type: "POST",
                    dataType: 'json',
                    data: JSON.stringify(orderRequests[distributionUrl]),
                    contentType: "application/json",
                    xhrFields: { withCredentials: IsGeonorge(distributionUrl) },
                    async: false,
                    error: function (jqXHR, textStatus, errorThrown) {
                        showAlert(errorThrown, "danger");
                        responseFailed = true;
                    },
                    success: function (data) {
                        if (data !== null) {
                            responseData.push(
                                {
                                    "distributionUrl": distributionUrl,
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

            if (!responseFailed) {
                this.removeAllOrderLines();
            }
            this.orderResponse = responseData;
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
        removeOrderLine: function (orderLineUuid) {
            for (property in this.masterOrderLine) {
                if (this.masterOrderLine[property][orderLineUuid] !== undefined) {
                    delete this.masterOrderLine[property][orderLineUuid];
                }
            }

            this.orderLines.forEach(function (orderLine, index) {
                if (orderLine.metadata.uuid == orderLineUuid) {
                    this.orderLines.splice(index, 1);
                }
            }.bind(this));

            this.removeFromLocalStorage(orderLineUuid);
        },
        removeAllOrderLines: function () {
            var orderLineUuids = [];
            if (this.orderLines !== undefined && this.orderLines.length) {
                this.orderLines.forEach(function (orderLine) {
                    orderLineUuids.push(orderLine.metadata.uuid)
                });
                orderLineUuids.forEach(function (orderLineUuid) {
                    this.removeOrderLine(orderLineUuid);
                }.bind(this));
            }
            $('#remove-all-items-modal').modal('hide');
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
