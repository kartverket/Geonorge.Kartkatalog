var map;
var geoCoder;
var myApp = angular.module('searchApp', ['ngAnimate']);



String.prototype.contains = function (t) { return this.indexOf(t) != -1; };


myApp.controller('SearchController', function ($scope, $filter, $timeout) {
    $scope.countyList = countyList;
    $scope.storedSearches = [];
    $scope.lastSearchQuery = '';
    $scope.resultList = [];
    $scope.applicationsCount = 0;
    $scope.datasetsCount = 0;
    $scope.mapServicesCount = 0;
    $scope.showGeoLocation = false;
    $scope.searchActive = false;
    $scope.initLoad = false;
    $scope.showLoader = false;
    $scope.mapInitialized = false;
    $scope.totalCount = 0;
    $scope.filterContainers = {};
    $scope.workList = [];
    $scope.autoCompleteResult = [];
    $scope.baseurl = 'http://' + window.location.host;

    $scope.autoCompletePartial = 'http://' + window.location.host + '/Scripts/Geonorge/partials/_autoCompleteRow.html';
    $scope.storedSearchesPartial = 'http://' + window.location.host + '/Scripts/Geonorge/partials/_storedSearches.html';
    $scope.filterPartial = 'http://' + window.location.host + '/Scripts/Geonorge/partials/_filters.html';
    $scope.metadataDetailsPartial = 'http://' + window.location.host + '/Scripts/Geonorge/partials/_metadataDetails.html';

    $scope.displayGeoLocation = function () {
        $scope.showGeoLocation == true ? $scope.showGeoLocation = false : $scope.showGeoLocation = true;
        if ($scope.showGeoLocation) {
            if (!$scope.mapInitialized) {
                initializeMap();
                $scope.mapInitialized = true;
            }

        }
    };

    $scope.selectResultType = function () {
        
    };

    $scope.storeLastSearch = function () {
        if ($scope.lastSearchQuery == undefined || $scope.lastSearchQuery.length == 0)
            return;
        if (window.localStorage) {
            var search = {
                searchQuery: angular.copy($scope.searchQuery),
                filterContainers: angular.copy($scope.filterContainers)
            };

            var containers = [];
            var checkedContainers = [];
            for (var item in search.filterContainers) {
                var currType = search.filterContainers[item];
                if (!currType.visible) continue;

                if (containers.indexOf(currType.type) == -1) {
                    containers.push(currType.type);
                }
                for (var listItem in currType.list) {
                    var subType = currType.list[listItem];

                    for (var subTypeItem in subType.list) {
                        var currFilter = subType.list[subTypeItem];
                        //If filter is checked
                        if (currFilter.checked) {
                            //Push container type into containers if it doesnt already exist there
                            if (checkedContainers.indexOf(currType.type) == -1) {
                                checkedContainers.push(currType.type);
                            }
                        }
                    }
                }
            }
            if (checkedContainers.length > 0) {
                search.class = checkedContainers.length == 1 ? scopeStoredSearchLabel[checkedContainers[0]] : scopeStoredSearchLabel['mixed'];
                if (checkedContainers.length > 0) {
                    search.iconClasses = [];
                    for (var x in checkedContainers) {
                        var iconX = scopeFilterIcons[checkedContainers[x]];
                        search.iconClasses.push(iconX);
                    }
                }
            } else {
                search.class = containers.length == 1 ? scopeStoredSearchLabel[containers[0]] : scopeStoredSearchLabel['mixed'];
                if (containers.length > 0) {
                    search.iconClasses = [];
                    for (var i in containers) {
                        var icon = scopeFilterIcons[containers[i]];
                        search.iconClasses.push(icon);
                    }
                }
            }

            $scope.storedSearches.push(search);
            localStorage.searches = angular.toJson($scope.storedSearches);
            console.log($scope.storedSearches);
        } else {
            //cookie
        }
    };

    $scope.readStoredSearches = function () {
        if (window.localStorage) {
            if (localStorage.searches) {
                var searches = JSON.parse(localStorage.searches);
                $scope.storedSearches = [];
                for (var i = 0; i < searches.length; i++) {
                    $scope.storedSearches.push(angular.copy(searches[i]));
                }
            }
        }
    };

    $scope.restoreSearch = function (index) {
        $scope.searchActive = true;
        $scope.showLoader = true;
        var item;

        if ($scope.storedSearches[index]) {
            item = $scope.storedSearches[index];
            $scope.showLoader = true;
            $timeout(function () {
                $scope.searchQuery = item.searchQuery;
                performSearch();
                syncFilters(item.filterContainers);
                $scope.showLoader = false;
            }, 1000);
        }
    };

    function syncFilters(restored) {
        var filtersActive = false;
        for (container in $scope.filterContainers) {
            var reference = null;
            for (var i = 0; i < restored.length; i++) {
                if ($scope.filterContainers[container].type == restored[i].type) {
                    reference = restored[i];
                    break;
                }
            }
            if (reference == null) continue;

            $scope.filterContainers[container].visible = reference.visible;

            for (list in $scope.filterContainers[container].list) {
                for (filter in $scope.filterContainers[container].list[list].list) {

                    if (reference.list[list].list[filter].checked) filtersActive = true;
                    $scope.filterContainers[container].list[list].list[filter].checked = reference.list[list].list[filter].checked;
                    $scope.filterContainers[container].list[list].list[filter].containerOn = reference.list[list].list[filter].containerOn;
                }
            }

        }
        $scope.filtersActive = filtersActive;
    }

    $scope.removeStoredSearch = function (index) {
        var item;


        if ($scope.storedSearches[index]) {
            item = $scope.storedSearches[index];
            $scope.storedSearches.splice($scope.storedSearches.indexOf(item), 1);
            localStorage.searches = angular.toJson($scope.storedSearches);
        }
    };

    $scope.cleanStoredSearches = function () {
        localStorage.removeItem("searches");
        $scope.storedSearches = [];
    };

    $scope.addToWorkList = function (index) {
        $scope.workList.push($scope.resultList[index]);
    };

    $scope.removeFromWorkList = function (index) {
        var item = $scope.workList[index];
        $scope.workList.splice($scope.workList.indexOf(item), 1);
    };

    $scope.cleanWorkList = function () {
        $scope.workList = [];
    };

    $scope.Search = function () {
        $scope.autoCompleteResult = [];
        $scope.searchActive = true;
        $scope.showLoader = true;

        if (window.sessionStorage) {
            sessionStorage.runSearch = true;
        }

        $timeout(function () {
            performSearch();
        }, 1000);


    };

    function performSearch() {
        $scope.lastSearchQuery = $scope.searchQuery;
        storeLastSearch();
        $scope.resultList = [];
        $scope.filtersActive = false;
        var results = jointSearch();
        buildFilter(results);
        $scope.resultList = results;
        $scope.showLoader = false;

        if (!$scope.initLoad) {
            $timeout(function () { $scope.initLoad = true; }, 500);
        }

        $scope.calculateResultCount();
    }

    function storeLastSearch() {
        if ($scope.searchQuery != undefined) {
            if (window.sessionStorage) {
                sessionStorage.lastSearchQuery = $scope.searchQuery;
            } else {
                //TODO: Cookie
            }
        }
        else {
            sessionStorage.removeItem("lastSearchQuery");
        }
    }

    function jointSearch() {
        var result = [];
        var resultMetadata = $scope.filterResults(mockDataMetaCatalogue, $scope.searchQuery);
        result = result.concat(resultMetadata);
        var resultKartverket = $scope.filterResults(mockDataKartverket, $scope.searchQuery);
        result = result.concat(resultKartverket);
        var resultObjectCatalogue = $scope.filterResults(mockDataObjectCatalogue, $scope.searchQuery);
        result = result.concat(resultObjectCatalogue);
        return result;
    }
    var categoryCount = null;
    var resultCount = null;
    var shiftKey = false;
    $scope.preventDefault = function (ev) {

        switch (ev.keyCode) {
            case 13:
                if (categoryCount != null || !$scope.searchQuery) {
                    ev.preventDefault();
                }
                break;
            case 16:
                shiftKey = true;
                break;
            case 9:
                if ($scope.autoCompleteResult.length > 0) {
                    ev.preventDefault();
                }
                break;
            case 38:
            case 40:
                ev.preventDefault();
                break;
        }
    };

    function setHighlightedRow() {
        for (item in $scope.autoCompleteResult) {
            if (parseInt(item) == categoryCount - 1) {
                for (innerItem in $scope.autoCompleteResult[item].list) {
                    if (parseInt(innerItem) == resultCount - 1) {
                        $scope.autoCompleteResult[item].list[innerItem].highlight = true;
                        angular.element($scope.autoCompleteResult[item].list[innerItem]).focus();

                    } else {
                        $scope.autoCompleteResult[item].list[innerItem].highlight = false;
                    }
                }

            } else {

                for (xx in $scope.autoCompleteResult[parseInt(item)].list) {

                    $scope.autoCompleteResult[item].list[xx].highlight = false;
                }
            }
        }
    }

    $scope.isFirstItem = function(obj) {
        var result = false;
        var items = [];
        var curr = {};
        for (item in $scope.resultList) {
            if ($scope.resultList[item].visible == true) {
                if ($scope.resultList[item].typeId == obj.typeId) {
                    items.push($scope.resultList[item]);
                }
            }
        }
        if (items.indexOf(obj) == 0) result = true;

        return result;
    };

    $scope.autocomplete = function (ev) {
        if (!$scope.searchQuery) {
            return;
        }
        if ($scope.searchQuery.length < 3) {
            $scope.autoCompleteResult = [];
            categoryCount = null;
            return;
        }

        switch (ev.keyCode) {
            //enter
            case 13:
                if (categoryCount == null) {
                    $scope.autoCompleteResult = [];

                } else {
                    if ($scope.autoCompleteResult[categoryCount - 1]) {
                        $scope.goToAutoCompleteResult($scope.autoCompleteResult[categoryCount - 1].list[resultCount - 1]);
                        $scope.autoCompleteResult = [];
                        categoryCount = null;
                    }
                }
                break;
            case 16:
                shiftKey = false;
                break;
                //left
            case 37:
                break;
                //up
            case 38:

                autoCompleteMoveUp();

                return false;
                //right
            case 39:
                break;
                //Tab
            case 9:
                
                    if (!shiftKey) {
                        autoCompleteMoveDown();
                    } else {
                        autoCompleteMoveUp();
                    }
                break;
                //down
            case 40:
                autoCompleteMoveDown();

                return false;
            default:
                categoryCount = null;

                var results = jointSearch();
                results = results.splice(0, 10);
                $scope.autoCompleteResult = [];

                for (var i = 0; i < results.length; i++) {
                    results[i].highlight = false;
                    var item = {};
                    if (i == 0) {
                        item.type = results[i].typeId,
                        item.title = scopeEnumsAutocomplete[results[i].typeId];
                        item.list = [];
                        item.list.push({
                            id: results[i].id,
                            typeId: results[i].typeId,
                            title: results[i].title,
                            url: results[i].url
                        });
                        $scope.autoCompleteResult.push(item);
                    } else {

                        for (var x = 0; x < $scope.autoCompleteResult.length; x++) {
                            var hit = false;
                            if ($scope.autoCompleteResult[x].type == results[i].typeId) {
                                $scope.autoCompleteResult[x].list.push({
                                    id: results[i].id,
                                    typeId: results[i].typeId,
                                    title: results[i].title,
                                    url: results[i].url
                                });
                                hit = true;
                            }
                        }
                        if (!hit) {
                            $scope.autoCompleteResult.push({
                                type: results[i].typeId,
                                title: scopeEnumsAutocomplete[results[i].typeId],
                                list: [{ id: results[i].id, typeId: results[i].typeId, title: results[i].title, url: results[i].url }]
                            });

                        }

                    }

                }
                break;
        }
    };

    function autoCompleteMoveUp() {

        if (resultCount > 0 && categoryCount == 1) {
            resultCount--;
            if (resultCount == 0) categoryCount = null;
        }

        if (resultCount == 1 && categoryCount > 1) {
            categoryCount--;
            resultCount = $scope.autoCompleteResult[categoryCount - 1].list.length;
        }



        if (categoryCount > 1 & resultCount > 1) {
            resultCount--;
        }

        setHighlightedRow();
    }

    function autoCompleteMoveDown() {
        if (categoryCount == null) {
            categoryCount = 1;
            resultCount = 1;
        } else {
            if (categoryCount == $scope.autoCompleteResult.length) {
                if ($scope.autoCompleteResult[categoryCount - 1].list.length > resultCount) {
                    resultCount++;
                }
            }
            if (categoryCount < $scope.autoCompleteResult.length) {
                if ($scope.autoCompleteResult[categoryCount - 1].list.length > resultCount) {
                    resultCount++;
                }
                else {
                    categoryCount++;
                    resultCount = 1;
                }
            }
        }
        setHighlightedRow();
    }

    $scope.getStatusClass = function (status) {
        switch (status.toLowerCase()) {
            case 'vedtatt':
                return 'label-vedtatt';
            case 'under arbeid':
                return 'label-underarbeid';
            case 'godkjent':
                return 'label-godkjent';

            default:
                return null;
        }
    }

    $scope.goToAutoCompleteResult = function (el) {
        switch (el.typeId) {
            case 'kartverket':
                $scope.redirect(el.url);
                break;
            case 'metadata':
                var hash = '?id=' + el.id;
                $scope.redirect($scope.baseurl + '/home/metadata/' + hash);
                break;
            case 'object_catalogue':
                $scope.redirect(el.url);
                break;
        }
    };

    $scope.setFocus = function (ev) {
        angular.element(ev.target).on('blur', function () {
            $timeout(function () {
                $scope.autoCompleteResult = [];
            }, true);
        });
    };

    $scope.getResultTemplate = function (typeId) {
        switch (typeId) {
            case 'kartverket':
                return 'http://' + window.location.host + '/partials/_kartverket.html';
            case 'metadata':
                return 'http://' + window.location.host + '/partials/_metadata.html';
            case 'object_catalogue':
                return 'http://' + window.location.host + '/partials/_objektkatalog.html';
            default:
                return null;
        }
    };

    var visibilityCount = 0;
    $scope.getResultVisibility = function (obj) {
        var visible = (obj.dependent.checked && $scope.filtersActive && obj.dependent.containerOn) || (!$scope.filtersActive && obj.dependent.containerOn);

        obj.visible = visible;
        visibilityCount++;
        if (visibilityCount == $scope.resultList.length) {
            $scope.calculateResultCount();
            visibilityCount = 0;
        }
        return visible;
    };


    function buildFilter(results) {
        $scope.filterContainers = [];
        var filters = [];
        for (var i = 0; i < results.length; i++) {


            var hit = false;
            for (var x = 0; x < filters.length; x++) {

                //Level 1
                if (filters[x].type == results[i].typeId) {
                    var typeHit = false;

                    for (var y = 0; y < filters[x].list.length; y++) {
                        //Level 2
                        if (filters[x].list[y].subType == results[i].subTypeId) {
                            var subTypeHit = false;
                            for (var z = 0; z < filters[x].list[y].list.length; z++) {
                                //Level 3
                                if (filters[x].list[y].list[z].type == results[i].type) {
                                    filters[x].list[y].list[z].count++;
                                    subTypeHit = true;
                                    results[i].dependent = filters[x].list[y].list[z];
                                    results[i].dependent.containerOn = true;
                                    break;
                                }
                            }
                            if (!subTypeHit) {
                                var item = {
                                    typeId: results[i].typeId,
                                    type: results[i].type,
                                    checked: false,
                                    count: 1
                                };
                                filters[x].list[y].list.push(item);
                                results[i].dependent = item;
                                results[i].dependent.containerOn = true;
                            }

                            typeHit = true;
                            break;
                        }
                    }
                    if (!typeHit) {
                        var item = {
                            typeId: results[i].typeId,
                            type: results[i].type,
                            checked: false,
                            count: 1
                        };
                        filters[x].list.push({
                            subType: results[i].subTypeId,
                            list: [item]
                        });
                        results[i].dependent = item;
                        results[i].dependent.containerOn = true;
                    }
                    hit = true;
                    break;
                }
            }
            if (!hit) {
                var item = {
                    typeId: results[i].typeId,
                    type: results[i].type,
                    checked: false,
                    count: 1
                };
                filters.push({
                    label: scopeFilterLabel[results[i].typeId],
                    type: results[i].typeId,
                    title: scopeEnumsFilter[results[i].typeId],
                    icon: scopeFilterIcons[results[i].typeId],
                    list: [{
                        subType: results[i].subTypeId,
                        list: [item]
                    }],
                    visible: true
                });
                results[i].dependent = item;
                results[i].dependent.containerOn = true;
            }
        }
        $scope.filterContainers = filters;
    }

    $scope.filterResults = function (data, query) {
        for (var item in data) {
            data[item].class = scopeCategoryColor[data[item].typeId];
                data[item].icon = scopeFilterIcons[data[item].typeId];
        }

        if (query == '' || query == undefined) {
            return data;
        }

        var list = [];
        var match = false;
        for (var key in data) {

            for (var prop in data[key]) {

                if (data[key].hasOwnProperty(prop)) {
                    var value = data[key][prop];
                    if (value.toString().toLowerCase().contains(query.trim().toLowerCase())) {
                        list.push(data[key]);
                        break;
                    }
                }
            }
        }

        return list;
    };

    $scope.setCountyBounds = function () {
        geoCoder.geocode({ 'address': $scope.county }, function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {

                map.fitBounds(results[0].geometry.bounds);
                map.setZoom(7);
            }
        });
    };

    $scope.calculateResultCount = function () {
        var count = 0;
        for (var i = 0; i < $scope.resultList.length; i++) {
            if ($scope.resultList[i].visible) count++;
        }
        $scope.totalCount = count;
    };



    $scope.disableFilterContainer = function (obj) {


        if (obj.visible == true) {
            for (var x = 0; x < obj.list.length; x++) {
                for (var y = 0; y < obj.list[x].list.length; y++) {
                    obj.list[x].list[y].checked = false;
                    obj.list[x].list[y].containerOn = false;
                }
            }
        } else {
            for (var x = 0; x < obj.list.length; x++) {
                for (var y = 0; y < obj.list[x].list.length; y++) {
                    obj.list[x].list[y].checked = false;
                    obj.list[x].list[y].containerOn = true;
                }
            }
        }
        obj.visible = obj.visible == true ? false : true;

        setFilterState();
    };

    $scope.disableType = function (typeId) {

        $scope.filterContainers[typeId] = $scope.filterContainers[typeId] ? false : true;
        var mode = $scope.filterContainers[typeId];
        for (x in $scope.resultList) {
            if ($scope.resultList[x].typeId.toLowerCase() == typeId.toLowerCase()) {
                $scope.resultList[x].visible = mode;

                if ($scope.filters[$scope.resultList[x].type.toLowerCase()]) {
                    $scope.filters[$scope.resultList[x].type.toLowerCase()] = false;
                }
            }
        }
        $scope.DisplayType();
    };

    $scope.emptyFilters = function () {

        $scope.resetFilters();
        //$scope.DisplayType();

    };

    $scope.resetFilters = function () {
        for (container in $scope.filterContainers) {
            $scope.filterContainers[container].visible = true;
            for (list in $scope.filterContainers[container].list) {
                for (filter in $scope.filterContainers[container].list[list].list) {
                    $scope.filterContainers[container].list[list].list[filter].checked = false;
                    $scope.filterContainers[container].list[list].list[filter].containerOn = true;
                }
            }
        }
        $scope.filtersActive = false;
    };

    $scope.filtersActive = false;

    $scope.showFilter = function (filter) {
        if (filter.checked) {
            $scope.filtersActive = true;
        } else {
            setFilterState();
        }
    };
    function setFilterState() {
        var activeFilters = false;
        for (container in $scope.filterContainers) {
            for (list in $scope.filterContainers[container].list) {
                for (filter in $scope.filterContainers[container].list[list].list) {
                    if ($scope.filterContainers[container].list[list].list[filter].checked == true) {
                        activeFilters = true;
                        break;
                    }

                }
            }
        }
        $scope.filtersActive = activeFilters;
        //setIcon($scope.resultList);
    }


    $scope.TypeFilter = function ($event) {
        var cbx = $event.target;
    };

    $scope.focusSearch = function () {

        $scope.searchStyle = {};
        //$timeout(function () {
        //    $scope.searchActive = true;
        //}, 500);
    };

    $scope.redirect = function (url) {
        window.location = url;
    };

    $scope.checkForLastSearchQuery = function () {
        if (window.sessionStorage) {
            if (sessionStorage.lastSearchQuery) {
                var query = sessionStorage.lastSearchQuery;
                $scope.searchQuery = query;
                $scope.Search();
                //TODO: refill filters as well
            } else {
                if (sessionStorage.runSearch) {
                    if (sessionStorage.runSearch == 'true') {
                        $scope.Search();
                    }
                }
            }
        } else {
            //TODO: cookie
        }
    };

    $scope.readStoredSearches();
    $scope.checkForLastSearchQuery();
});



function initializeMap() {

    geoCoder = new google.maps.Geocoder();

    var mapOptions = {
        zoom: 3,
        center: new google.maps.LatLng(63.430515, 10.395053)
    };
    map = new google.maps.Map(document.getElementById('map-canvas'),
        mapOptions);
    var drawingManager = new google.maps.drawing.DrawingManager({
        drawingMode: google.maps.drawing.OverlayType.POLYGON,
        drawingControl: true,
        drawingControlOptions: {
            position: google.maps.ControlPosition.TOP_CENTER,
            drawingModes: [
              google.maps.drawing.OverlayType.CIRCLE,
              google.maps.drawing.OverlayType.POLYGON,
              google.maps.drawing.OverlayType.RECTANGLE
            ]
        },
        markerOptions: {
            icon: 'images/beachflag.png'
        },
        circleOptions: {
            fillColor: '#ffff00',
            fillOpacity: 1,
            strokeWeight: 5,
            clickable: false,
            editable: true,
            zIndex: 1
        },
        polygonOptions: {
            draggable: true,
            editable: true
        }
    });
    drawingManager.setMap(map);

    google.maps.event.addListener(drawingManager, 'polygoncomplete', function (polygon) {

        polygon.addListener('dragend', function (e) {
            var len1 = polygon.getPaths().getLength();
            var len2 = polygon.getPath().getLength();
            for (var i = 0; i < len1; i++) {
                console.log(polygon.getPaths().getAt(i));
            }

            console.log('--------------getPath()----------------');
            for (var x = 0; x < len2; x++) {
                console.log(polygon.getPath().getAt(x));
            }
        });

        polygon.addListener('click', function () {
            this.setMap(null);
        });
    });

    google.maps.event.addListener(drawingManager, 'circlecomplete', function (circle) {
        console.log(circle.getBounds());

        map.setCenter(circle.getCenter());
    });
}



