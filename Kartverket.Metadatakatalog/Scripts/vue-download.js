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
                showAlert("Kunne ikke legge til " + name + " til nedlastning. Feilmelding: " + errorThrown + "<br/>", "danger");
                returnData = "error";
            },
            success: function (data) {
                if (data !== null) {
                    returnData = data;
                }
                else {
                    showAlert("Data mangler for å kunne lastes ned. Vennligst fjern " + name + " for nedlastning. <br/>", "danger");
                }
            }
        });
    }
    return returnData;
}


document.addEventListener("DOMContentLoaded", function (event) {
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

                                var isAllreadyAddedInfo = this.$root.isAllreadyAdded(this.$root.masterOrderLine.masterSelectedAreas, area, "code");
                                if (!isAllreadyAddedInfo.added) {
                                    this.$root.masterOrderLine.masterSelectedAreas.push(area);
                                }
                            }
                        }.bind(this));
                    }
                }
                this.$root.updateSelectedAreasForAllOrderLines(true);
                this.$root.updateNotAvailableSelectedAreasForAllOrderLines();

            } else {
                var orderLineUuid = this.$parent.metadata.uuid;
                if (this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type] !== undefined) {
                    this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type].forEach(function (availableArea, index) {
                        if (availableArea.code == area.code) {
                            this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type][index].isSelected = true;
                        }
                    }.bind(this));
                }
                this.$root.updateSelectedAreasForSingleOrderLine(orderLineUuid, true);
            }

            this.$root.validateAreas();
        },
        removeSelectedArea: function (area) {
            if (this.master) {
                for (orderLineUuid in this.$root.masterOrderLine.allAvailableAreas) {
                    if (this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type] !== undefined) {
                        // Unselect from order lines
                        this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type].forEach(function (availableArea, index) {
                            if (availableArea.code == area.code) {
                                this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type][index].isSelected = false;
                                this.$root.removePreSelectedAreaFromLocalStorage(orderLineUuid, area);
                            }
                        }.bind(this));
                    }

                    // Unselect from master order line
                    this.$root.masterOrderLine.masterSelectedAreas.forEach(function (selectedArea, index) {
                        if (selectedArea.code == area.code) {
                            this.$root.masterOrderLine.masterSelectedAreas.splice(index, 1)
                        }
                    }.bind(this));

                }
                this.$root.updateSelectedAreasForAllOrderLines(false);
                this.$root.updateNotAvailableSelectedAreasForAllOrderLines();

            } else {
                var orderLineUuid = this.$parent.metadata.uuid;
                if (this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type] !== undefined) {
                    this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type].forEach(function (availableArea, index) {
                        if (availableArea.code == area.code) {
                            this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][area.type][index].isSelected = false;
                            this.$root.removePreSelectedAreaFromLocalStorage(orderLineUuid, area);
                        }
                    }.bind(this));
                }
                this.$root.updateSelectedAreasForSingleOrderLine(orderLineUuid, false);
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

                                var isAllreadyAddedInfo = this.$root.isAllreadyAdded(this.$root.masterOrderLine.masterSelectedProjections, projection, "code");
                                if (!isAllreadyAddedInfo.added) {
                                    this.$root.masterOrderLine.masterSelectedProjections.push(projection);
                                }
                            }
                        }.bind(this));
                    }
                }
                this.$root.updateSelectedProjectionsForAllOrderLines();
                this.$root.updateNotAvailableSelectedProjectionsForAllOrderLines();

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
                        // Unselect from order lines
                        this.$root.masterOrderLine.allAvailableProjections[orderLineUuid].forEach(function (availableProjection, index) {
                            if (availableProjection.code == projection.code) {
                                this.$root.masterOrderLine.allAvailableProjections[orderLineUuid][index].isSelected = false;
                            }
                        }.bind(this));
                    }
                    // Unselect from master order line
                    this.$root.masterOrderLine.masterSelectedProjections.forEach(function (selectedProjection, index) {
                        if (selectedProjection.code == projection.code) {
                            this.$root.masterOrderLine.masterSelectedProjections.splice(index, 1)
                        }
                    }.bind(this));
                }
                this.$root.updateSelectedProjectionsForAllOrderLines();
                this.$root.updateNotAvailableSelectedProjectionsForAllOrderLines();
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
                                var isAllreadyAddedInfo = this.$root.isAllreadyAdded(this.$root.masterOrderLine.masterSelectedFormats, format, "name");
                                if (!isAllreadyAddedInfo.added) {
                                    this.$root.masterOrderLine.masterSelectedFormats.push(format);
                                }
                            }
                        }.bind(this));
                    }
                }
                this.$root.updateSelectedFormatsForAllOrderLines();
                this.$root.updateNotAvailableSelectedFormatsForAllOrderLines();
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
                        // Unselect from order lines
                        this.$root.masterOrderLine.allAvailableFormats[orderLineUuid].forEach(function (availableFormat, index) {
                            if (availableFormat.name == format.name) {
                                this.$root.masterOrderLine.allAvailableFormats[orderLineUuid][index].isSelected = false;

                            }
                        }.bind(this));
                    }
                    // Unselect from master order line
                    this.$root.masterOrderLine.masterSelectedFormats.forEach(function (selectedFormat, index) {
                        if (selectedFormat.name == format.name) {
                            this.$root.masterOrderLine.masterSelectedFormats.splice(index, 1)
                        }
                    }.bind(this));

                }
                this.$root.updateSelectedFormatsForAllOrderLines();
                this.$root.updateNotAvailableSelectedFormatsForAllOrderLines();
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
    props: ['metadata', 'capabilities', 'availableAreas', 'availableProjections', 'availableFormats', 'selectedAreas', 'selectedProjections', 'selectedFormats', 'selectedCoordinates', 'defaultProjections', 'defaultFormats', 'orderLineErrors', 'orderLineInfoMessages', 'notAvailableSelectedAreas', 'notAvailableSelectedProjections', 'notAvailableSelectedFormats'],
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
        hasAreas: function () {
            if (this.capabilities.supportsPolygonSelection) {
                return true;
            }
            for (var prop in this.availableAreas) {
                if (this.availableAreas.hasOwnProperty(prop)) {
                    return true;
                }
            }
            return false;
        },
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
                    if (errorType == 'area' && this.orderLineErrors[errorType].length) {
                        return 3;
                    } else if (this.orderLineErrors[errorType].length) {
                        numberOfErrors++;
                    }
                }
            }
            return numberOfErrors;
        },
        hasInfoMessages: function () {
            var hasInfoMessages = false;
            if (this.orderLineInfoMessages !== undefined && Object.keys(this.orderLineInfoMessages).length) {
                for (infoMessageType in this.orderLineInfoMessages) {
                    if (this.orderLineInfoMessages[infoMessageType].length) {
                        hasInfoMessages = true;
                    }
                }
            }
            if (this.notAvailableSelectedAreas.length) hasInfoMessages = true;
            return hasInfoMessages;
        },
        numberOfInfoMessages: function () {
            var numberOfInfoMessages = 0;
            if (this.orderLineInfoMessages !== undefined && Object.keys(this.orderLineInfoMessages).length) {
                for (infoMessageType in this.orderLineInfoMessages) {
                    if (this.orderLineInfoMessages[infoMessageType].length) {
                        numberOfInfoMessages++;
                    }
                }
            }
            if (this.notAvailableSelectedAreas.length) numberOfInfoMessages++;
            if (this.notAvailableSelectedProjections.length) numberOfInfoMessages++;
            if (this.notAvailableSelectedFormats.length) numberOfInfoMessages++;
            return numberOfInfoMessages;
        },
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
        selectFromMap: function (orderItem, mapType) {
            orderItem.showMap = true;
            if (!this.mapIsLoaded) {
                showLoadingAnimation("Henter kart");
            }
            var fixed = orderItem.capabilities.supportsGridSelection;
            if (mapType == "grid") { this.loadGridMap(orderItem) }
            else if (mapType == "polygon") { this.loadPolygonMap(orderItem) }
            $('#norgeskartmodal #setcoordinates').attr('uuid', orderItem.metadata.uuid);
        },

        selectAllGrids: function (orderItem) {

            var orderLineUuid = orderItem.metadata.uuid;

            orderItem.availableAreas.celle.forEach(function (selectedArea) {
                selectedArea.isSelected = true;
                orderItem.selectedAreas.push(selectedArea);
            }.bind(this));

            this.$root.updateSelectedAreasForSingleOrderLine(orderLineUuid, true);
            this.$root.updateAvailableProjectionsAndFormatsForSingleOrderLine(orderLineUuid);
            this.$root.validateAreas();   
        },

        isJson: function (str) {
            try {
                JSON.parse(str);
            } catch (e) {
                return false;
            }
            return true;
        },

        loadGridMap: function () {
            var orderItem = this;
            orderItem.mapIsLoaded = true;
            this.$root.activeMapUuid = orderItem.metadata.uuid;
            orderItem.mapData.defaultConfigurations = {
                "service_name": orderItem.capabilities.mapSelectionLayer,
                "center_longitude": "378604",
                "center_latitude": "7226208",
                "zoom_level": "3"
            }

            window.addEventListener('message', function (e) {
                if (e !== undefined && e.data !== undefined && typeof (e.data) == "string") {
                    var iframeElement = document.getElementById(orderItem.metadata.uuid + "-iframe");
                    var modalElement = $(iframeElement).closest(".custom-modal").removeClass("hidden");
                    hideLoadingAnimation();
                    var msg = JSON.parse(e.data);
                    if (msg.type === "mapInitialized") {
                        iframeMessage = {
                            "cmd": "setCenter",
                            "x": orderItem.mapData.defaultConfigurations.center_longitude,
                            "y": orderItem.mapData.defaultConfigurations.center_latitude,
                            "zoom": orderItem.mapData.defaultConfigurations.zoom_level
                        };
                        iframeElement.contentWindow.postMessage(JSON.stringify(iframeMessage), '*');

                        iframeMessage = {
                            "cmd": "setVisible",
                            "id": orderItem.mapData.defaultConfigurations.service_name
                        };
                        iframeElement.contentWindow.postMessage(JSON.stringify(iframeMessage), '*');

                    } else if (orderItem.metadata.uuid == this.$root.activeMapUuid) {
                        if (msg.cmd === "setVisible") return;
                        var obj = msg;
                        if (this.isJson(msg)) {
                            var data = JSON.parse(msg);
                            if (data["type"] == "mapInitialized") return;
                            var areaname = data["attributes"]["n"];
                            if (data["cmd"] == "featureSelected") {
                                for (areaType in this.$root.masterOrderLine.allAvailableAreas[this.$root.activeMapUuid]) {
                                    this.$root.masterOrderLine.allAvailableAreas[this.$root.activeMapUuid][areaType].forEach(function (availableArea) {
                                        if (availableArea.code == areaname) {
                                            availableArea.isSelected = true;
                                        }
                                    })
                                }
                            }
                            if (data["cmd"] == "featureUnselected") {
                                for (areaType in this.$root.masterOrderLine.allAvailableAreas[this.$root.activeMapUuid]) {
                                    this.$root.masterOrderLine.allAvailableAreas[this.$root.activeMapUuid][areaType].forEach(function (availableArea) {
                                        if (availableArea.code == areaname) {
                                            availableArea.isSelected = false;
                                        }
                                    })
                                }
                            }
                            this.$root.updateSelectedAreasForSingleOrderLine(this.$root.activeMapUuid, true);
                            this.$root.updateAvailableProjectionsAndFormatsForSingleOrderLine(this.$root.activeMapUuid);
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
                coordinateSystem: "25833",
                selection_type: "3525",
                service_name: "fylker-utm32",
                zoom_level: "4",
            }

            window.addEventListener('message', function (e) {
                if (e !== undefined && e.data !== undefined && typeof (e.data) == "string") {
                    var iframeElement = document.getElementById(orderItem.metadata.uuid + "-iframe");
                    var modalElement = $(iframeElement).closest(".custom-modal").removeClass("hidden");
                    hideLoadingAnimation();
                    var msg = JSON.parse(e.data);
                    if (msg.type === "mapInitialized") {
                        iframeMessage = {
                            "cmd": "setCenter",
                            "x": orderItem.mapData.defaultConfigurations.center_longitude,
                            "y": orderItem.mapData.defaultConfigurations.center_latitude,
                            "zoom": orderItem.mapData.defaultConfigurations.zoom_level
                        };
                        iframeElement.contentWindow.postMessage(JSON.stringify(iframeMessage), '*');
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
                                                if (data.message !== undefined && data.message !== null && data.message !== '') {
                                                    showAlert("Det oppstod et problem ved valg av område: " + data.message, "danger");
                                                }
                                                else {
                                                    showAlert("Området er for stort til å laste ned, vennligst velg mindre område", "danger");
                                                }
                                            } else {
                                                clearAlertMessage();
                                                hideAlert();

                                                this.$root.masterOrderLine.allSelectedCoordinates[orderItem.metadata.uuid] = coordinatesString;
                                                var polygonArea = {
                                                    "name": SelectedFromMap,
                                                    "type": "polygon",
                                                    "code": "Kart",
                                                    "isSelected": true,
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

                                                this.$root.updateSelectedAreasForSingleOrderLine(orderItem.metadata.uuid, true);
                                                this.$root.updateAvailableProjectionsAndFormatsForSingleOrderLine(orderItem.metadata.uuid);
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
    mounted: function () {
        this.expanded = this.$root.orderLines.length == 1;
    }
};

var MasterOrderLine = {
    props: ['allAvailableAreas', 'availableAreas', 'availableProjections', 'availableFormats', 'allSelectedAreas', 'allSelectedProjections', 'allSelectedFormats', 'selectedAreas', 'selectedProjections', 'selectedFormats', 'allOrderLineErrors'],
    data: function () {
        var data = {

            showAreaHelpText: false,
            showProjectionHelpText: false,
            showFormatHelpText: false,

            mapData: {},
            mapIsLoaded: false,
            showMap: false,

            master: true
        }
        return data;
    },
    created: function () {

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
            if (!this.mapIsLoaded) {
                showLoadingAnimation("Henter kart");
            }
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
                coordinateSystem: "25833",
                selection_type: "3525",
                service_name: "fylker-utm32",
                zoom_level: "4",
            }

            window.addEventListener('message', function (e) {
                if (e !== undefined && e.data !== undefined && typeof (e.data) == "string") {
                    var iframeElement = document.getElementById("masterorderline-iframe");
                    var modalElement = $(iframeElement).closest(".custom-modal").removeClass("hidden");
                    hideLoadingAnimation();
                    var msg = JSON.parse(e.data);
                    if (msg.type === "mapInitialized") {
                        iframeMessage = {
                            "cmd": "setCenter",
                            "x": this.mapData.defaultConfigurations.center_longitude,
                            "y": this.mapData.defaultConfigurations.center_latitude,
                            "zoom": this.mapData.defaultConfigurations.zoom_level
                        };
                        iframeElement.contentWindow.postMessage(JSON.stringify(iframeMessage), '*');
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
                                                if (data.message !== undefined && data.message !== null && data.message !== '')
                                                {
                                                    showAlert("Det oppstod et problem ved valg av område: " + data.message , "danger");
                                                }
                                                else {
                                                    showAlert("Området er for stort til å laste ned, vennligst velg mindre område", "danger");
                                                }
                                            } else {
                                                clearAlertMessage();
                                                hideAlert();

                                                mainVueModel.$children.forEach(function (orderItem) {
                                                    if (orderItem.master !== undefined && orderItem.master == false) {
                                                        if (orderItem.capabilities !== undefined && orderItem.capabilities.supportsPolygonSelection !== undefined && orderItem.capabilities.supportsPolygonSelection == true) {

                                                            this.$root.masterOrderLine.allSelectedCoordinates[orderItem.metadata.uuid] = coordinatesString;
                                                            var polygonArea = {
                                                                "name": SelectedFromMap,
                                                                "type": "polygon",
                                                                "code": "Kart",
                                                                "isSelected": true,
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

                                                this.$root.updateSelectedAreasForAllOrderLines(true);
                                                this.$root.updateAvailableProjectionsAndFormatsForAllOrderLines();
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
        usageGroup: localStorage.getItem('preSelectedUsageGroup') !== null ? JSON.parse(localStorage.getItem('preSelectedUsageGroup')) : "",
        usageGroupsAvailable: downloadUseGroups,
        usagePurposes: localStorage.getItem('preSelectedUsagePurposes') !== null ? JSON.parse(localStorage.getItem('preSelectedUsagePurposes')) : [] ,
        usagePurposesAvailable: downloadPurposes,
        softwareClient : downloadSoftwareClient,
        softwareClientVersion : downloadSoftwareClientVersion,
        orderRequests: {},
        orderRequestsAdditionalInfo: {},
        orderResponse: {},
        emailRequired: false,
        contentLoaded: false,
        activeMapUuid: false,

        masterOrderLine: {
            allAvailableAreas: {},
            masterAvailableAreas: {},
            allAvailableProjections: {},
            masterAvailableProjections: [],
            allAvailableFormats: {},
            masterAvailableFormats: [],
            allSelectedAreas: {},
            allSelectedProjections: {},
            allSelectedFormats: {},
            allSelectedCoordinates: {},
            masterSelectedAreas: [],
            masterSelectedProjections: [],
            masterSelectedFormats: [],
            allOrderLineErrors: {},
            allOrderLineInfoMessages: {},
            allDefaultProjections: {},
            allDefaultFormats: {},
            allNotAvailableSelectedAreas: {},
            allNotAvailableSelectedProjections: {},
            allNotAvailableSelectedFormats: {}
        }
    },
    computed: {
        orderResponseGrouped: function () {
            var orderResponseGrouped = [];
            if (this.orderResponse.length) {
                this.orderResponse.forEach(function (distribution) {
                    var orderResponseGroup = {
                        distributionUrl: distribution.distributionUrl,
                        datasets: {},
                        additionalInfo: { distributedBy: "Geonorge", supportsDownloadBundling: false },
                        orderBundleUrl: null,
                        numberOfFiles: distribution.data.files ? distribution.data.files.length : 0
                    };

                    if (distribution.data) {

                        if (distribution.data._links && distribution.data._links.length) {
                            distribution.data._links.forEach(function (link) {
                                if (link.rel === 'http://rel.geonorge.no/download/order/bundle') {
                                    orderResponseGroup.orderBundleUrl = link.href;
                                    return;
                                }
                            });
                        }

                        if (distribution.data.files && distribution.data.files.length) {
                            distribution.data.files.forEach(function (file) {
                                if (!orderResponseGroup.datasets[file.metadataName]) {
                                    orderResponseGroup.datasets[file.metadataName] = {
                                        files: []
                                    };
                                }
                                orderResponseGroup.datasets[file.metadataName].files.push(file);

                                orderResponseGroup.additionalInfo = this.getOrderRequestAdditionalInfo(distribution.distributionUrl, file.metadataUuid);

                            }.bind(this));
                        }

                    }


                    orderResponseGrouped.push(orderResponseGroup);
                }.bind(this));
            }
            return orderResponseGrouped;
        },
        showMasterOrderLine: function () {
            var availableAreaTypes = Object.keys(this.masterOrderLine.masterAvailableAreas);
            var containsSupportedType = this.containsSupportedType(availableAreaTypes) || this.containsLandsdekkende(availableAreaTypes);
            return this.orderLines.length > 1 && containsSupportedType;
        }
    },
    created: function () {
        var defaultUrl = "https://nedlasting.geonorge.no/api/capabilities/";
        var orderItemsJson = localStorage["orderItems"] ? JSON.parse(localStorage["orderItems"]) : [];
        var orderLines = [];
       
        if (orderItemsJson.length) {
            var key = 0;
            $(orderItemsJson).each(function (index, val) {
                if (val !== undefined && val !== null && val !== "") {
                    var metadata = localStorage[val + ".metadata"] ? JSON.parse(localStorage[val + ".metadata"]) : "";
                    var apiUrl = metadata.distributionUrl ? metadata.distributionUrl : defaultUrl;
                    var capabilities = metadata.capabilities ? metadata.capabilities : getJsonData(apiUrl + val);
                    if (capabilities === "error" && metadata !== "") {
                        this.removeOrderLine(metadata.uuid, false);
                    }
                    else if (capabilities !== "") {
                        orderLines[key] = {
                            "metadata": metadata,
                            "capabilities": capabilities,
                            "projectionAndFormatIsRequired": false
                        };

                        if (metadata !== "" && metadata.uuid !== undefined && metadata.uuid !== "") {

                            var uuid = metadata.uuid;

                            this.masterOrderLine.allAvailableProjections[uuid] = [];
                            this.masterOrderLine.allAvailableFormats[uuid] = [];

                            this.masterOrderLine.allSelectedAreas[uuid] = [];
                            this.masterOrderLine.allSelectedProjections[uuid] = [];

                            this.masterOrderLine.allSelectedFormats[uuid] = [];
                            this.masterOrderLine.allSelectedCoordinates[uuid] = "";

                            this.masterOrderLine.allDefaultProjections[uuid] = [];
                            this.masterOrderLine.allDefaultFormats[uuid] = [];

                            if (orderLines[key].capabilities._links !== undefined && orderLines[key].capabilities._links.length) {
                                orderLines[key].capabilities._links.forEach(function (link) {
                                    if (link.rel === "http://rel.geonorge.no/download/order") {
                                        orderLines[key].metadata.orderDistributionUrl = link.href;
                                    }
                                    if (link.rel === "http://rel.geonorge.no/download/can-download") {
                                        orderLines[key].metadata.canDownloadUrl = link.href;
                                    }
                                    if (link.rel === "http://rel.geonorge.no/download/area") {
                                        var availableAreas = metadata.areas && metadata.areas.length ? metadata.areas : getJsonData(link.href);
                                        this.masterOrderLine.allAvailableAreas[uuid] = {};

                                        availableAreas.forEach(function (availableArea) {
                                            if (!this.masterOrderLine.allAvailableAreas[uuid][availableArea.type]) {
                                                this.masterOrderLine.allAvailableAreas[uuid][availableArea.type] = [];
                                            }
                                            availableArea.isSelected = false;
                                            this.masterOrderLine.allAvailableAreas[uuid][availableArea.type].push(availableArea);
                                        }.bind(this));

                                        //start set fixed sort order for area types

                                        var orderedAreas = ["fylke", "kommune", "celle"];

                                        //Add available area types not fixed sorted
                                        for (areaType in this.masterOrderLine.allAvailableAreas[uuid]) {
                                            if (orderedAreas.indexOf(areaType) === -1)
                                                orderedAreas.push(areaType);
                                        }

                                        //Remove fixed area types not available
                                        var notAvailableAreaTypes = [];
                                        orderedAreas.forEach(function (areaType) {
                                            if (!this.masterOrderLine.allAvailableAreas[uuid][areaType]) {
                                                notAvailableAreaTypes.push(areaType);
                                            }
                                        }.bind(this));
                                        removeFromArray(orderedAreas, notAvailableAreaTypes);

                                        //Re-organize according to fixed order
                                        var allAvailableAreasForUuid = this.masterOrderLine.allAvailableAreas[uuid];
                                        this.masterOrderLine.allAvailableAreas[uuid] = {};

                                        for (keyType in orderedAreas) {
                                            areaType = orderedAreas[keyType];
                                            allAvailableAreasForUuid[areaType].forEach(function (availableArea) {
                                                if (!this.masterOrderLine.allAvailableAreas[uuid][areaType]) {
                                                    this.masterOrderLine.allAvailableAreas[uuid][areaType] = [];
                                                }
                                                availableArea.isSelected = false;
                                                this.masterOrderLine.allAvailableAreas[uuid][areaType].push(availableArea);
                                            }.bind(this));
                                        }

                                        //end set fixed sort order

                                    }
                                    if (link.rel === "http://rel.geonorge.no/download/projection") {
                                        var defaultProjections = metadata.projections && metadata.projections.length ? metadata.projections : getJsonData(link.href)
                                        orderLines[key].defaultProjections = defaultProjections;
                                        this.masterOrderLine.allDefaultProjections[uuid] = defaultProjections;
                                    }
                                    if (link.rel === "http://rel.geonorge.no/download/format") {
                                        var defaultFormats = metadata.formats && metadata.formats.length ? metadata.formats : getJsonData(link.href);
                                        orderLines[key].defaultFormats = defaultFormats;
                                        this.masterOrderLine.allDefaultFormats[uuid] = defaultFormats;
                                    }
                                }.bind(this));
                            }
                        }
                        var distributionUrl = (orderLines[key].metadata.distributionUrl !== undefined) ? orderLines[key].metadata.distributionUrl : "";

                        orderLines[key].capabilities.supportsGridSelection = orderLines[key].capabilities.mapSelectionLayer && orderLines[key].capabilities.mapSelectionLayer !== "" ? true : false;
                    }
                    key += 1;
                }

            }.bind(this));
        }
        this.orderLines = orderLines;
        this.updateAvailableAreasForMasterOrderLine();
        this.updateAvailableProjectionsAndFormatsForAllOrderLines();
        this.updateAvailableProjectionsAndFormatsForMasterOrderLine();
    },
    mounted: function () {
        this.autoselectWithOrderLineValuesFromLocalStorage();
        this.autoselectWithMasterOrderLineValuesFromLocalStorage();
        this.autoSelectAreasForAllOrderLines();
        this.updateSelectedAreasForAllOrderLines(true);
        this.updateNotAvailableSelectedAreasForAllOrderLines();
        this.updateNotAvailableSelectedProjectionsForAllOrderLines();
        this.updateNotAvailableSelectedFormatsForAllOrderLines();
        this.validateAreas();
        this.contentLoaded = true;
    },
    components: {
        'orderLine': OrderLine,
        'masterOrderLine': MasterOrderLine
    },
    methods: {
        getPurposeName: function (purpose) {

            for (var item in this.usagePurposesAvailable)
                if (purpose === item)
                    return this.usagePurposesAvailable[item];

            return purpose;
        },
        purposeIsSelected: function (purpose) {
            for (var item in this.usagePurposes) {
                if (purpose === this.usagePurposes[item])
                    return true;
            }
            return false;
        },
        selectPurpose: function (purposeName, purpose) {
            var purposeAdded = false;
            for (var i = 0; i < this.usagePurposes.length; i++) {
                if (this.usagePurposes[i] === purpose) {
                    purposeAdded = true;
                }
            }

            if (!purposeAdded)
                this.usagePurposes.push(purpose);

        },

        removeSelectedPurpose: function (purpose) {
            this.usagePurposes.splice(purpose, 1);
        }
        ,
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
        containsSupportedType: function (areaTypes) {
            var containsSupportedType = false;
            areaTypes.forEach(function (areaType) {
                if (this.isSupportedType(areaType)) {
                    containsSupportedType = true;
                    return;
                }
            }.bind(this));
            return containsSupportedType;
        },
        containsLandsdekkende: function (areaTypes) {
            var containsLandsdekkende = false;
            areaTypes.forEach(function (areaType) {
                if (areaType == 'landsdekkende') {
                    containsLandsdekkende = true;
                    return;
                }
            }.bind(this));
            return containsLandsdekkende;
        },
        hasMasterSelectedProjections: function () {
            return this.masterOrderLine.masterSelectedProjections.length > 0;
        },
        hasMasterSelectedFormats: function () {
            return this.masterOrderLine.masterSelectedFormats.length > 0;
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
                                if (selectedFormat.projections !== undefined) {
                                    //check if selectedFormat.projections is in allSelectedProjections 
                                    selectedFormat.projections.forEach(function (projection) {
                                        this.masterOrderLine.allSelectedProjections[orderLine].forEach(function (projectionSelected) {
                                            if (projectionSelected.code == projection.code)
                                                hasSelectedFormats = true

                                        }.bind(this))

                                    }.bind(this))
                                }
                                else {
                                    hasSelectedFormats = true
                                }
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
        hasSelectedProjectionsDifferentFromMasterSelectedProjections: function (orderLineUuid) {
            if (this.hasMasterSelectedProjections()) {
                this.masterOrderLine.allSelectedProjections[orderLineUuid].forEach(function (selectedProjection) {
                    var isMasterSelected = false;
                    this.masterOrderLine.masterSelectedProjections.forEach(function (masterSelectedProjection) {
                        if (masterSelectedProjection.code == selectedProjection.code) {
                            isMasterSelected = true;
                        }
                    }.bind(this));
                    if (!isMasterSelected) {
                        var infoMessage = "" + selectedProjection.name + " er ikke valgt som fellesvalg";
                        this.masterOrderLine.allOrderLineInfoMessages[orderLineUuid]["projection"].push(infoMessage);
                    }
                }.bind(this));
            }
        },
        hasSelectedFormatsDifferentFromMasterSelectedFormats: function (orderLineUuid) {
            if (this.hasMasterSelectedFormats()) {
                this.masterOrderLine.allSelectedFormats[orderLineUuid].forEach(function (selectedFormat) {
                    var isMasterSelected = false;
                    this.masterOrderLine.masterSelectedFormats.forEach(function (masterSelectedFormat) {
                        if (masterSelectedFormat.name == selectedFormat.name) {
                            isMasterSelected = true;
                        }
                    }.bind(this));
                    if (!isMasterSelected) {
                        var infoMessage = "" + selectedFormat.name + " er ikke valgt som fellesvalg";
                        this.masterOrderLine.allOrderLineInfoMessages[orderLineUuid]["format"].push(infoMessage);
                    }
                }.bind(this));
            }
        },
        updateInfoMessagesForOrderLine: function (orderLineUuid) {
            this.masterOrderLine.allOrderLineInfoMessages[orderLineUuid] = {};
            this.masterOrderLine.allOrderLineInfoMessages[orderLineUuid]["projection"] = [];
            this.masterOrderLine.allOrderLineInfoMessages[orderLineUuid]["format"] = [];
            this.masterOrderLine.allOrderLineInfoMessages[orderLineUuid]["area"] = [];

            this.hasSelectedProjectionsDifferentFromMasterSelectedProjections(orderLineUuid);
            this.hasSelectedFormatsDifferentFromMasterSelectedFormats(orderLineUuid);

        },
        validateAreas: function () {
            var emailRequired = false;
            for (orderLineUuid in this.masterOrderLine.allAvailableAreas) {
                this.masterOrderLine.allOrderLineErrors[orderLineUuid] = {};
                this.masterOrderLine.allOrderLineErrors[orderLineUuid]["projection"] = [];
                this.masterOrderLine.allOrderLineErrors[orderLineUuid]["format"] = [];
                this.masterOrderLine.allOrderLineErrors[orderLineUuid]["area"] = [];
                if (this.masterOrderLine.allSelectedAreas[orderLineUuid] !== undefined && this.masterOrderLine.allSelectedAreas[orderLineUuid].length) {

                    this.masterOrderLine.allSelectedAreas[orderLineUuid].forEach(function (selectedArea) {
                        selectedArea.hasSelectedProjections = this.hasSelectedProjections(selectedArea, orderLineUuid);
                        selectedArea.hasSelectedFormats = this.hasSelectedFormats(selectedArea, orderLineUuid);

                        this.orderLines.forEach(function (orderLine) {
                            if (orderLine.metadata.uuid == orderLineUuid && !emailRequired) {
                                emailRequired = orderLine.capabilities.deliveryNotificationByEmail !== undefined ? orderLine.capabilities.deliveryNotificationByEmail : false;
                            }
                        }.bind(this));
                        if (selectedArea.type == "polygon") {
                            emailRequired = true;
                        }
                    }.bind(this));

                } else {
                    this.masterOrderLine.allOrderLineErrors[orderLineUuid]["area"] = ["Datasett mangler valgt område"];
                }
                this.updateInfoMessagesForOrderLine(orderLineUuid);
                this.updateSelectedAreasForSingleOrderLine(orderLineUuid, false);
            }
            this.emailRequired = emailRequired;
            this.updateOrderRequests();
            this.addSelectedOrderLineValuesToLocalStorage();
            this.addSelectedMasterOrderLineValuesToLocalStorage();
            setTimeout(function () {
                $("[data-toggle='tooltip']").tooltip();
            }, 300);
        },

        updateNotAvailableSelectedAreasForSingleOrderLine: function (orderLineUuid) {
            var notAvailableSelectedAreas = [];
            this.$root.masterOrderLine.masterSelectedAreas.forEach(function (masterSelectedArea) {
                var notAvailableForOrderLine = true;
                if (this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][masterSelectedArea.type] !== undefined) {
                    this.$root.masterOrderLine.allAvailableAreas[orderLineUuid][masterSelectedArea.type].forEach(function (availableArea, index) {
                        if (availableArea.code == masterSelectedArea.code) {
                            notAvailableForOrderLine = false;
                        }
                    });
                }
                if (notAvailableForOrderLine) {
                    notAvailableSelectedAreas.push(masterSelectedArea);
                }
            }.bind(this));
            this.$root.masterOrderLine.allNotAvailableSelectedAreas[orderLineUuid] = notAvailableSelectedAreas;
        },

        updateNotAvailableSelectedAreasForAllOrderLines: function () {
            this.orderLines.forEach(function (orderLine) {
                if (orderLine.metadata !== undefined && orderLine.metadata.uuid !== undefined) {
                    this.updateNotAvailableSelectedAreasForSingleOrderLine(orderLine.metadata.uuid);
                }
            }.bind(this));
        },

        updateNotAvailableSelectedProjectionsForSingleOrderLine: function (orderLineUuid) {
            var notAvailableSelectedProjections = [];
            this.$root.masterOrderLine.masterSelectedProjections.forEach(function (masterSelectedProjection) {
                var notAvailableForOrderLine = true;
                if (this.$root.masterOrderLine.allAvailableProjections[orderLineUuid].length) {
                    this.$root.masterOrderLine.allAvailableProjections[orderLineUuid].forEach(function (availableProjection, index) {
                        if (availableProjection.code == masterSelectedProjection.code) {
                            notAvailableForOrderLine = false;
                        }
                    }.bind(this));
                }
                if (notAvailableForOrderLine) {
                    notAvailableSelectedProjections.push(masterSelectedProjection)
                }
            }.bind(this));
            this.$root.masterOrderLine.allNotAvailableSelectedProjections[orderLineUuid] = notAvailableSelectedProjections;
        },

        updateNotAvailableSelectedProjectionsForAllOrderLines: function () {
            this.orderLines.forEach(function (orderLine) {
                if (orderLine.metadata !== undefined && orderLine.metadata.uuid !== undefined) {
                    this.updateNotAvailableSelectedProjectionsForSingleOrderLine(orderLine.metadata.uuid);
                }
            }.bind(this));
        },

        updateNotAvailableSelectedFormatsForSingleOrderLine: function (orderLineUuid) {
            var notAvailableSelectedFormats = [];
            this.$root.masterOrderLine.masterSelectedFormats.forEach(function (masterSelectedFormat) {
                var notAvailableForOrderLine = true;
                if (this.$root.masterOrderLine.allAvailableFormats[orderLineUuid].length) {
                    this.$root.masterOrderLine.allAvailableFormats[orderLineUuid].forEach(function (availableFormat, index) {
                        if (availableFormat.name == masterSelectedFormat.name) {
                            notAvailableForOrderLine = false;
                        }
                    }.bind(this));
                }
                if (notAvailableForOrderLine) {
                    notAvailableSelectedFormats.push(masterSelectedFormat)
                }
            }.bind(this));
            this.$root.masterOrderLine.allNotAvailableSelectedFormats[orderLineUuid] = notAvailableSelectedFormats;
        },

        updateNotAvailableSelectedFormatsForAllOrderLines: function (orderLineUuid) {
            this.orderLines.forEach(function (orderLine) {
                if (orderLine.metadata !== undefined && orderLine.metadata.uuid !== undefined) {
                    this.updateNotAvailableSelectedFormatsForSingleOrderLine(orderLine.metadata.uuid);
                }
            }.bind(this));
        },

        updateAvailableProjectionsAndFormatsForSingleOrderLine: function (orderLineUuid) {
            var availableProjections = [];
            var availableFormats = [];

            if (this.masterOrderLine.allSelectedAreas[orderLineUuid].length) {
                this.masterOrderLine.allSelectedAreas[orderLineUuid].forEach(function (selectedArea) {

                    // Get available projections
                    if (selectedArea.allAvailableProjections !== undefined && selectedArea.allAvailableProjections[orderLineUuid] !== undefined && selectedArea.allAvailableProjections[orderLineUuid].length) {
                        selectedArea.allAvailableProjections[orderLineUuid].forEach(function (availableProjection) {
                            var isAllreadyAddedInfo = this.isAllreadyAdded(availableProjections, availableProjection, "code");
                            if (!isAllreadyAddedInfo.added) {
                                availableProjections.push(availableProjection);
                            }
                        }.bind(this))
                    }

                    // Get available formats
                    if (selectedArea.allAvailableFormats !== undefined && selectedArea.allAvailableFormats[orderLineUuid] !== undefined && selectedArea.allAvailableFormats[orderLineUuid].length) {
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

        updateAvailableProjectionsAndFormatsForAllOrderLines: function () {
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


        updateAvailableAreasForMasterOrderLine: function () {
            var masterAvailableAreas = {};
            for (orderLine in this.masterOrderLine.allAvailableAreas) {
                for (areaType in this.masterOrderLine.allAvailableAreas[orderLine]) {
                    this.masterOrderLine.allAvailableAreas[orderLine][areaType].forEach(function (area) {
                        if (masterAvailableAreas[areaType] == undefined) {
                            masterAvailableAreas[areaType] = [];
                        }

                        if (area.allAvailableProjections == undefined) { area.allAvailableProjections = {} };
                        if (area.allAvailableProjections[orderLine] == undefined) { area.allAvailableProjections[orderLine] = [] };
                        area.allAvailableProjections[orderLine] = area.projections;

                        if (area.allAvailableFormats == undefined) { area.allAvailableFormats = {} };
                        if (area.allAvailableFormats[orderLine] == undefined) { area.allAvailableFormats[orderLine] = [] };
                        area.allAvailableFormats[orderLine] = area.formats;

                        var areaIsAllreadyAddedInfo = this.isAllreadyAdded(masterAvailableAreas[areaType], area, "code");

                        if (!areaIsAllreadyAddedInfo.added) {
                            masterAvailableAreas[areaType].push(area);
                        }
                    }.bind(this))
                }
            }
            this.masterOrderLine.masterAvailableAreas = masterAvailableAreas;
        },

        removeUnavailableSelectedAreasForMasterOrderLine: function () {
            var masterSelectedAreas = [];
            this.masterOrderLine.masterSelectedAreas.forEach(function (masterSelectedArea) {
                var isAvailable = false;
                for (areaType in this.masterOrderLine.masterAvailableAreas) {
                    this.masterOrderLine.masterAvailableAreas[areaType].forEach(function (masterAvailableArea) {
                        if (masterSelectedArea.code == masterAvailableArea.code) {
                            isAvailable = true;
                        }
                    });
                }
                if (isAvailable) {
                    masterSelectedAreas.push(masterSelectedArea);
                }
            }.bind(this));
            this.masterOrderLine.masterSelectedAreas = masterSelectedAreas;
        },

        removeUnavailableSelectedProjectionsForMasterOrderLine: function () {
            var masterSelectedProjections = [];
            this.masterOrderLine.masterSelectedProjections.forEach(function (masterSelectedProjection) {
                var isAvailable = false;
                this.masterOrderLine.masterAvailableProjections.forEach(function (masterAvailableProjection) {
                    if (masterSelectedProjection.code == masterAvailableProjection.code) {
                        isAvailable = true;
                    }
                });
                if (isAvailable) {
                    masterSelectedProjections.push(masterSelectedProjection);
                }
            }.bind(this));
            this.masterOrderLine.masterSelectedProjections = masterSelectedProjections;
        },

        removeUnavailableSelectedFormatsForMasterOrderLine: function () {
            var masterSelectedFormats = [];
            this.masterOrderLine.masterSelectedFormats.forEach(function (masterSelectedFormat) {
                var isAvailable = false;
                this.masterOrderLine.masterAvailableFormats.forEach(function (masterAvailableFormat) {
                    if (masterSelectedFormat.name == masterAvailableFormat.name) {
                        isAvailable = true;
                    }
                });
                if (isAvailable) {
                    masterSelectedFormats.push(masterSelectedFormat);
                }
            }.bind(this));
            this.masterOrderLine.masterSelectedFormats = masterSelectedFormats;
        },


        updateSelectedAreasForSingleOrderLine: function (orderLineUuid, autoSelectProjectionsAndFormats) {
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
            if (autoSelectProjectionsAndFormats !== undefined && autoSelectProjectionsAndFormats === true) {
                this.autoSelectProjectionAndFormatsForSingleOrderLine(orderLineUuid);
            }
            this.updateSelectedProjectionsForSingleOrderLine(orderLineUuid);
            this.updateSelectedFormatsForSingleOrderLine(orderLineUuid);
            this.updateAvailableProjectionsAndFormatsForMasterOrderLine();
        },


        updateSelectedProjectionsForSingleOrderLine: function (orderLineUuid) {
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

        updateSelectedFormatsForSingleOrderLine: function (orderLineUuid) {
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

        updateSelectedAreasForAllOrderLines: function (autoSelectProjectionsAndFormats) {
            this.orderLines.forEach(function (orderLine) {
                if (orderLine.metadata !== undefined && orderLine.metadata.uuid !== undefined) {
                    this.updateSelectedAreasForSingleOrderLine(orderLine.metadata.uuid, autoSelectProjectionsAndFormats);
                }
            }.bind(this));
        },

        updateSelectedProjectionsForAllOrderLines: function () {
            this.orderLines.forEach(function (orderLine) {
                if (orderLine.metadata !== undefined && orderLine.metadata.uuid !== undefined) {
                    this.updateSelectedProjectionsForSingleOrderLine(orderLine.metadata.uuid);
                }
            }.bind(this));
        },

        updateSelectedFormatsForAllOrderLines: function () {
            this.orderLines.forEach(function (orderLine) {
                if (orderLine.metadata !== undefined && orderLine.metadata.uuid !== undefined) {
                    this.updateSelectedFormatsForSingleOrderLine(orderLine.metadata.uuid);
                }
            }.bind(this));
        },

        autoSelectAreasForSingleOrderLine: function (orderLineUuid) {
            for (areaType in this.masterOrderLine.allAvailableAreas[orderLineUuid]) {
                if (this.masterOrderLine.allAvailableAreas[orderLineUuid][areaType].length) {
                    this.masterOrderLine.allAvailableAreas[orderLineUuid][areaType].forEach(function (availableArea, index) {
                        if (this.masterOrderLine.masterSelectedAreas.length) {
                            this.masterOrderLine.masterSelectedAreas.forEach(function (masterSelectedArea) {
                                if (masterSelectedArea.type == "polygon") {
                                    this.masterOrderLine.allSelectedCoordinates[orderLineUuid] = masterSelectedArea.coordinates;
                                    if (this.masterOrderLine.allAvailableAreas[orderLineUuid]['polygon'] == undefined) {
                                        this.masterOrderLine.allAvailableAreas[orderLineUuid]['polygon'] = [];
                                        this.masterOrderLine.allAvailableAreas[orderLineUuid]['polygon'].push(masterSelectedArea);
                                    }
                                }
                                if (masterSelectedArea.code == availableArea.code) {
                                    this.masterOrderLine.allAvailableAreas[orderLineUuid][areaType][index].isSelected = true;
                                }
                            }.bind(this));
                        }
                    }.bind(this))
                }
            }
        },

        autoSelectAreasForAllOrderLines: function () {
            this.orderLines.forEach(function (orderLine) {
                if (orderLine.metadata !== undefined && orderLine.metadata.uuid !== undefined) {
                    this.autoSelectAreasForSingleOrderLine(orderLine.metadata.uuid);
                }
            }.bind(this));
        },

        autoSelectProjectionAndFormatsForSingleOrderLine: function (orderLineUuid) {
            this.masterOrderLine.allSelectedAreas[orderLineUuid].forEach(function (selectedArea) {
                if (selectedArea && selectedArea.allAvailableProjections && selectedArea.allAvailableProjections[orderLineUuid] && selectedArea.allAvailableProjections[orderLineUuid].length && selectedArea.allAvailableProjections[orderLineUuid].length == 1) {
                    var projectionCode = selectedArea.allAvailableProjections[orderLineUuid][0].code;
                    this.masterOrderLine.allAvailableProjections[orderLineUuid].forEach(function (availableProjection, index) {
                        if (availableProjection.code == projectionCode) {
                            this.masterOrderLine.allAvailableProjections[orderLineUuid][index].isSelected = true;
                        }
                    }.bind(this));
                }
                if (selectedArea && selectedArea.allAvailableFormats && selectedArea.allAvailableFormats[orderLineUuid] && selectedArea.allAvailableFormats[orderLineUuid].length && selectedArea.allAvailableFormats[orderLineUuid].length == 1) {
                    var formatName = selectedArea.allAvailableFormats[orderLineUuid][0].name;
                    this.masterOrderLine.allAvailableFormats[orderLineUuid].forEach(function (availableFormat, index) {
                        if (availableFormat.name == formatName) {
                            this.masterOrderLine.allAvailableFormats[orderLineUuid][index].isSelected = true;
                        }
                    }.bind(this));
                }
            }.bind(this));
            this.updateNotAvailableSelectedProjectionsForSingleOrderLine(orderLineUuid);
            this.updateNotAvailableSelectedFormatsForSingleOrderLine(orderLineUuid);
        },
        updateOrderRequests: function () {
            var orderRequests = {};
            var orderRequestsAdditionalInfo = {};
            if (this.orderLines.length) {
                this.orderLines.forEach(function (orderLine) {
                    if (orderLine.metadata !== undefined) {
                        if (orderRequests[orderLine.metadata.orderDistributionUrl] == undefined) {
                            orderRequests[orderLine.metadata.orderDistributionUrl] = {
                                "email": "",
                                "usageGroup": this.usageGroup,
                                "softwareClient": this.softwareClient,
                                "softwareClientVersion": this.softwareClientVersion,
                                "orderLines": []
                            }
                        }

                        var areas = [];
                        if (this.masterOrderLine.allSelectedAreas[orderLine.metadata.uuid] !== undefined && this.masterOrderLine.allSelectedAreas[orderLine.metadata.uuid].length) {
                            this.masterOrderLine.allSelectedAreas[orderLine.metadata.uuid].forEach(function (selectedArea) {
                                var area = {
                                    "code": selectedArea.code,
                                    "name": selectedArea.name,
                                    "type": selectedArea.type,
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
                                }
                                formats.push(format);
                            });
                        }

                        var orderRequest = {
                            "metadataUuid": orderLine.metadata.uuid,
                            "areas": areas,
                            "projections": projections,
                            "formats": formats,
                            "usagePurpose": this.usagePurposes
                        }

                        if (this.masterOrderLine.allSelectedCoordinates[orderLine.metadata.uuid] !== "") {
                            orderRequest.coordinates = this.masterOrderLine.allSelectedCoordinates[orderLine.metadata.uuid];
                        }

                        orderRequests[orderLine.metadata.orderDistributionUrl].orderLines.push(orderRequest);

                        if (orderRequestsAdditionalInfo[orderLine.metadata.orderDistributionUrl] == undefined) {
                            orderRequestsAdditionalInfo[orderLine.metadata.orderDistributionUrl] = {};
                        }
                        if (orderRequestsAdditionalInfo[orderLine.metadata.orderDistributionUrl][orderLine.metadata.uuid] == undefined) {
                            orderRequestsAdditionalInfo[orderLine.metadata.orderDistributionUrl][orderLine.metadata.uuid] = {};
                        }
                        orderRequestsAdditionalInfo[orderLine.metadata.orderDistributionUrl][orderLine.metadata.uuid] = {
                            "distributedBy": orderLine.capabilities.distributedBy,
                            "supportsDownloadBundling": orderLine.capabilities.supportsDownloadBundling
                        }


                    }
                }.bind(this));
            }
            this.orderRequests = orderRequests;
            this.orderRequestsAdditionalInfo = orderRequestsAdditionalInfo;
        },
        getOrderRequestAdditionalInfo: function (distributionUrl, metadataUuid) {
            return this.orderRequestsAdditionalInfo[distributionUrl] != undefined && this.orderRequestsAdditionalInfo[distributionUrl][metadataUuid] != undefined ? this.orderRequestsAdditionalInfo[distributionUrl][metadataUuid] : {};
        },
        getCookie: function(cname) {
            var name = cname + '=';
            var ca = document.cookie.split(';');
            for(var i = 0; i<ca.length; i++) {
                var c = ca[i];
                while (c.charAt(0) === ' ') {
                    c = c.substring(1);
                }
                if (c.indexOf(name) === 0) {
                    return c.substring(name.length, c.length);
                }
            }
            return '';
        },
        sendRequests: function () {
            this.updateUsageForOrderRequests();
            this.updateEmailForOrderRequests();
            var responseData = [];
            var responseFailed = false;
            var orderRequests = this.orderRequests;
            var bearerToken = this.getCookie('oidcAccessToken');
            for (distributionUrl in orderRequests) {

                $.ajax({
                    url: distributionUrl,
                    type: "POST",
                    dataType: 'json',
                    data: JSON.stringify(orderRequests[distributionUrl]),
                    contentType: "application/json",
                    xhrFields: { withCredentials: IsGeonorge(distributionUrl) },
                    async: false,
                    beforeSend: function (xhr) {
                        if (bearerToken && bearerToken.length) {
                            xhr.setRequestHeader('Authorization', 'Bearer ' + bearerToken);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        if (jqXHR.status === 500) {
                            showAlert(jqXHR.responseText, "danger");
                        } else {
                            showAlert(errorThrown, "danger");
                        }
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
                this.removeSelectedOrderLineValuesFromLocalStorage();
                this.removeSelectedMasterOrderLineValuesFromLocalStorage();
            }
            this.orderResponse = responseData;
        },

        sendOrderBundleRequest: function (responseItem) {
            if (this.emailAddressIsValid(this.email)) {
                var emailAddress = this.email;
                $('#order-bundle-button-' + responseItem.additionalInfo.distributedBy).addClass("disabled");
                showLoadingAnimation("Laster..");
                var data = {
                    email: emailAddress,
                    downloadAsBundle: true
                };
                var bearerToken = this.getCookie('oidcAccessToken');
                $.ajax({
                    url: responseItem.orderBundleUrl,
                    type: 'PUT',
                    dataType: 'json',
                    contentType: "application/json",
                    xhrFields: { withCredentials: IsGeonorge(responseItem.distributionUrl) },
                    data: JSON.stringify(data),
                    beforeSend: function (xhr) {
                        if (bearerToken && bearerToken.length) {
                            xhr.setRequestHeader('Authorization', 'Bearer ' + bearerToken);
                        }
                    },
                    success: function (data) {
                        hideLoadingAnimation();
                        $('#order-bundle-message-' + responseItem.additionalInfo.distributedBy).addClass("alert alert-success");
                        $('#order-bundle-message-' + responseItem.additionalInfo.distributedBy).text('Pakken med alle datasett vil bli sendt til ' + emailAddress + ' så snart den er klar');
                    },
                    error: function (xhr, status, errorThrown) {
                        hideLoadingAnimation();
                        showAlert("Feil: " + errorThrown, 'danger');
                    }
                });
            }
        },
        AddAccessToken: function (fileUrl) {
            var bearerToken = this.getCookie('oidcAccessToken');

            if (bearerToken) {
                if (bearerToken.indexOf('?') > -1)
                    fileUrl = fileUrl + '&access_token=' + bearerToken;
                else
                    fileUrl = fileUrl + '?access_token=' + bearerToken;
            }
            return fileUrl;
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
        removeOrderLine: function (orderLineUuid, insideLoop) {
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
            if (!insideLoop) {
                this.updateAvailableAreasForMasterOrderLine();
                this.removeUnavailableSelectedAreasForMasterOrderLine();

                this.updateAvailableProjectionsAndFormatsForMasterOrderLine();
                this.removeUnavailableSelectedProjectionsForMasterOrderLine();
                this.removeUnavailableSelectedFormatsForMasterOrderLine();
                this.updateOrderRequests();
                this.addSelectedOrderLineValuesToLocalStorage();
                this.addSelectedMasterOrderLineValuesToLocalStorage();
            }
        },
        removeAllOrderLines: function () {
            var orderLineUuids = [];
            if (this.orderLines !== undefined && this.orderLines.length) {
                this.orderLines.forEach(function (orderLine) {
                    orderLineUuids.push(orderLine.metadata.uuid)
                });
                orderLineUuids.forEach(function (orderLineUuid) {
                    this.removeOrderLine(orderLineUuid, true);
                }.bind(this));
            }
            $('#remove-all-items-modal').modal('hide');
            this.updateAvailableAreasForMasterOrderLine();
            this.removeUnavailableSelectedAreasForMasterOrderLine();

            this.updateAvailableProjectionsAndFormatsForMasterOrderLine();
            this.removeUnavailableSelectedProjectionsForMasterOrderLine();
            this.removeUnavailableSelectedFormatsForMasterOrderLine();
            this.addSelectedOrderLineValuesToLocalStorage();
            this.addSelectedMasterOrderLineValuesToLocalStorage();
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
            var regex = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
            return regex.test(email);
        },
        checkEmailAddress: function () {
            $('#feedback-alert .message').html('');
            hideAlert();
            var emailAddress = this.email;
            var validEmail = this.emailAddressIsValid(emailAddress);
            if (!validEmail && emailAddress) {
                showAlert("Epost-adresse er ugyldig<br/>", "danger");
            }
            return validEmail;
        },
        isEmpty: function (item) {
            return (item == null || item == undefined || item.length == 0);
        },
        forHasNoErrors: function () {
            var forHasNoErrors = true;
            var errorCount = 0;

            for (orderLineUuid in this.masterOrderLine.allOrderLineErrors) {
                var orderLineErrors = this.masterOrderLine.allOrderLineErrors[orderLineUuid];
                var orderLineErrorsCount = 0;
                for (errorType in orderLineErrors) {
                    orderLineErrorsCount += orderLineErrors[errorType].length;
                }
                errorCount += orderLineErrorsCount;
            }

            if (errorCount > 0) { forHasNoErrors = false }
            return forHasNoErrors;
        },

        formIsValid: function () {
            var emailFieldNotEmpty = (this.email !== "") ? true : false;
            var emailAddressIsValid = this.emailAddressIsValid(this.email);
            var formHasNoErrors = this.forHasNoErrors();
            var emailRequired = this.emailRequired;
            var usageGroupFieldNotEmpty = (this.usageGroup !== "") ? true : false;
            var usagePurposeNotEmpty = Object.keys(this.usagePurposes).length === 0 ? false : true;
            var usageNotEmpty = usageGroupFieldNotEmpty && usagePurposeNotEmpty;
            var formIsValid = (((emailFieldNotEmpty && emailRequired && emailAddressIsValid && formHasNoErrors) || (!emailRequired && formHasNoErrors)) ? true : false) && usageNotEmpty;
            return formIsValid;
        },
        projectionAndFormatIsRequired: function (orderItem) {
            var required = this.orderItemHasCoordinates(orderItem);
            return required;
        },

        updateEmailForOrderRequests: function () {
            if (this.orderRequests !== undefined) {
                for (orderDistributionUrl in this.orderRequests) {
                    this.orderRequests[orderDistributionUrl].email = this.email;
                }
            }
        },

        updateUsageForOrderRequests: function () {
            if (this.orderRequests !== undefined) {
                for (orderDistributionUrl in this.orderRequests) {
                    localStorage.setItem('preSelectedUsageGroup', JSON.stringify(this.usageGroup));
                    this.orderRequests[orderDistributionUrl].usageGroup = this.usageGroup;
                    this.orderRequests[orderDistributionUrl].softwareClient = this.softwareClient;
                    this.orderRequests[orderDistributionUrl].softwareClientVersion = this.softwareClientVersion;
                    localStorage.setItem('preSelectedUsagePurposes', JSON.stringify(this.usagePurposes));
                    for (o = 0; o < this.orderRequests[orderDistributionUrl].orderLines.length; o++)
                        this.orderRequests[orderDistributionUrl].orderLines[o].usagePurpose = this.usagePurposes;
                }
            }
        },

        // Save selected order line values in local storage
        addSelectedOrderLineValuesToLocalStorage: function () {
            localStorage.setItem('allSelectedAreas', JSON.stringify(this.masterOrderLine.allSelectedAreas));
            localStorage.setItem('allSelectedProjections', JSON.stringify(this.masterOrderLine.allSelectedProjections));
            localStorage.setItem('allSelectedFormats', JSON.stringify(this.masterOrderLine.allSelectedFormats));
            localStorage.setItem('allSelectedCoordinates', JSON.stringify(this.masterOrderLine.allSelectedCoordinates));
        },
        getSelectedOrderLineValuesFromLocalStorage: function () {
            var selectedOrderLineValues = {
                allSelectedAreas: localStorage.getItem('allSelectedAreas') !== null ? JSON.parse(localStorage.getItem('allSelectedAreas')) : null,
                allSelectedProjections: localStorage.getItem('allSelectedProjections') !== null ? JSON.parse(localStorage.getItem('allSelectedProjections')) : null,
                allSelectedFormats: localStorage.getItem('allSelectedFormats') !== null ? JSON.parse(localStorage.getItem('allSelectedFormats')) : null,
                allSelectedCoordinates: localStorage.getItem('allSelectedCoordinates') !== null ? JSON.parse(localStorage.getItem('allSelectedCoordinates')) : null
            }
            return selectedOrderLineValues;
        },
        removeSelectedOrderLineValuesFromLocalStorage: function () {
            localStorage.removeItem('allSelectedAreas');
            localStorage.removeItem('allSelectedProjections');
            localStorage.removeItem('allSelectedFormats');
            localStorage.removeItem('allSelectedCoordinates');
        },
        getPreSelectedOrderLineValuesFromLocalStorage: function () {
            var preSelectedOrderLineValues = {
                preSelectedAreas: localStorage.getItem('preSelectedAreas') !== null ? JSON.parse(localStorage.getItem('preSelectedAreas')) : null
            };
            return preSelectedOrderLineValues;
        },
        removePreSelectedAreaFromLocalStorage: function (orderLineUuid, area) {
            var preSelectedAreas = localStorage.getItem('preSelectedAreas') !== null ? JSON.parse(localStorage.getItem('preSelectedAreas')) : null;
            var isPreSelectedArea = preSelectedAreas !== null && preSelectedAreas[orderLineUuid] !== undefined && preSelectedAreas[orderLineUuid].code == area.code;

            if (isPreSelectedArea) {
                delete preSelectedAreas[orderLineUuid];
                localStorage.setItem('preSelectedAreas', JSON.stringify(preSelectedAreas));
            }
        },
        autoselectWithOrderLineValuesFromLocalStorage: function () {
            var selectedOrderLineValues = this.getSelectedOrderLineValuesFromLocalStorage();
            var preSelectedOrderLineValues = this.getPreSelectedOrderLineValuesFromLocalStorage();


            // Autoselect areas
            for (orderLineUuid in this.masterOrderLine.allAvailableAreas) {
                var hasSelectedAreas = selectedOrderLineValues.allSelectedAreas !== null && selectedOrderLineValues.allSelectedAreas[orderLineUuid] !== undefined && selectedOrderLineValues.allSelectedAreas[orderLineUuid].length;
                var hasPreSelecteAreas = preSelectedOrderLineValues.preSelectedAreas !== null && preSelectedOrderLineValues.preSelectedAreas[orderLineUuid] !== undefined;

                if (hasSelectedAreas || hasPreSelecteAreas) {
                    for (areaType in this.masterOrderLine.allAvailableAreas[orderLineUuid]) {
                        if (hasSelectedAreas) {
                            selectedOrderLineValues.allSelectedAreas[orderLineUuid].forEach(function (selectedArea) {
                                if (selectedArea.type == 'polygon') {
                                    this.masterOrderLine.allAvailableAreas[orderLineUuid]['polygon'] = [];
                                    this.masterOrderLine.allAvailableAreas[orderLineUuid]['polygon'].push(selectedArea);
                                    this.masterOrderLine.allSelectedCoordinates[orderLineUuid] = selectedArea.coordinates;
                                } else {
                                    this.masterOrderLine.allAvailableAreas[orderLineUuid][areaType].forEach(function (availableArea, index) {
                                        if (availableArea.code == selectedArea.code) {
                                            this.masterOrderLine.allAvailableAreas[orderLineUuid][areaType][index].isSelected = true;
                                        }
                                    }.bind(this));
                                }
                            }.bind(this));
                        }

                        // Autoselected area from "hva-finnes-i-kommunen-eller-fylket":
                        if (hasPreSelecteAreas) {
                            var preSelectedArea = preSelectedOrderLineValues.preSelectedAreas[orderLineUuid];
                            this.masterOrderLine.allAvailableAreas[orderLineUuid][areaType].forEach(function (availableArea, index) {
                                if (availableArea.code == preSelectedArea.code) {
                                    this.masterOrderLine.allAvailableAreas[orderLineUuid][areaType][index].isSelected = true;

                                    // Autoselected area from "hva-finnes-i-kommunen-eller-fylket" to master order line
                                    var isAllreadyAddedInfo = this.isAllreadyAdded(this.masterOrderLine.masterSelectedAreas, availableArea, "code");
                                    if (!isAllreadyAddedInfo.added) {
                                        this.$root.masterOrderLine.masterSelectedAreas.push(availableArea);
                                    }

                                }
                            }.bind(this));
                        }

                    }
                }
            }

            this.updateSelectedAreasForAllOrderLines();
            this.updateAvailableProjectionsAndFormatsForAllOrderLines();
            this.updateAvailableProjectionsAndFormatsForMasterOrderLine();

            // Autoselect projections
            for (orderLineUuid in this.masterOrderLine.allAvailableProjections) {
                if (selectedOrderLineValues.allSelectedProjections !== null && selectedOrderLineValues.allSelectedProjections[orderLineUuid] !== undefined && selectedOrderLineValues.allSelectedProjections[orderLineUuid].length) {
                    this.masterOrderLine.allAvailableProjections[orderLineUuid].forEach(function (availableProjection, index) {
                        selectedOrderLineValues.allSelectedProjections[orderLineUuid].forEach(function (selectedProjection) {
                            if (availableProjection.code == selectedProjection.code) {
                                this.masterOrderLine.allAvailableProjections[orderLineUuid][index].isSelected = true;
                            }
                        }.bind(this));
                    }.bind(this));

                }
            }

            // Autoselect formats
            for (orderLineUuid in this.masterOrderLine.allAvailableFormats) {
                if (selectedOrderLineValues.allSelectedFormats !== null && selectedOrderLineValues.allSelectedFormats[orderLineUuid] !== undefined && selectedOrderLineValues.allSelectedFormats[orderLineUuid].length) {
                    this.masterOrderLine.allAvailableFormats[orderLineUuid].forEach(function (availableFormat, index) {
                        selectedOrderLineValues.allSelectedFormats[orderLineUuid].forEach(function (selectedFormat) {
                            if (availableFormat.code == selectedFormat.code) {
                                this.masterOrderLine.allAvailableFormats[orderLineUuid][index].isSelected = true;
                            }
                        }.bind(this));
                    }.bind(this));

                }
            }

            this.updateSelectedProjectionsForAllOrderLines();
            this.updateSelectedFormatsForAllOrderLines();
        },

        // Save selected master order line values in local storage
        addSelectedMasterOrderLineValuesToLocalStorage: function () {
            localStorage.setItem('masterSelectedAreas', JSON.stringify(this.masterOrderLine.masterSelectedAreas));
            localStorage.setItem('masterSelectedProjections', JSON.stringify(this.masterOrderLine.masterSelectedProjections));
            localStorage.setItem('masterSelectedFormats', JSON.stringify(this.masterOrderLine.masterSelectedFormats));
        },
        getSelectedMasterOrderLineValuesFromLocalStorage: function () {
            var selectedMasterOrderLineValues = {
                masterSelectedAreas: localStorage.getItem('masterSelectedAreas') !== null ? JSON.parse(localStorage.getItem('masterSelectedAreas')) : null,
                masterSelectedProjections: localStorage.getItem('masterSelectedProjections') !== null ? JSON.parse(localStorage.getItem('masterSelectedProjections')) : null,
                masterSelectedFormats: localStorage.getItem('masterSelectedFormats') !== null ? JSON.parse(localStorage.getItem('masterSelectedFormats')) : null,
            }

            return selectedMasterOrderLineValues;
        },
        removeSelectedMasterOrderLineValuesFromLocalStorage: function () {
            localStorage.removeItem('masterSelectedAreas');
            localStorage.removeItem('masterSelectedProjections');
            localStorage.removeItem('masterSelectedFormats');
        },
        autoselectWithMasterOrderLineValuesFromLocalStorage: function () {
            var selectedMasterOrderLineValues = this.getSelectedMasterOrderLineValuesFromLocalStorage();

            // Autoselect areas
            if (selectedMasterOrderLineValues.masterSelectedAreas !== null && selectedMasterOrderLineValues.masterSelectedAreas.length) {
                this.masterOrderLine.masterSelectedAreas = selectedMasterOrderLineValues.masterSelectedAreas;
            }

            // Autoselect projections
            if (selectedMasterOrderLineValues.masterSelectedProjections !== null && selectedMasterOrderLineValues.masterSelectedProjections.length) {
                this.masterOrderLine.masterSelectedProjections = selectedMasterOrderLineValues.masterSelectedProjections;
            }

            // Autoselect formats
            if (selectedMasterOrderLineValues.masterSelectedFormats !== null && selectedMasterOrderLineValues.masterSelectedFormats.length) {
                this.masterOrderLine.masterSelectedFormats = selectedMasterOrderLineValues.masterSelectedFormats;
            }
        },
    }
});
