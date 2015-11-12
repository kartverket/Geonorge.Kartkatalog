
(function () {

var app = angular.module('geonorge');

//myApp.run(function ($rootScope, $templateCache) {
//    $rootScope.$on('$viewContentLoaded', function () {
//        $templateCache.removeAll();
//    });
//});


var scopeFilterIcons = {
    //Geonorge
    0: 'icon-norgeskart',
    //Metadatakatalog
    1: 'icon-box',
    //Objektkatalog
    2: 'icon-layers'
};

var scopeFilterLabel = {
    //Geonorge
    0: 'label-geonorge',
    //Metadatakatalog
    1: 'label-metadata',
    //Objektkatalog
    2: 'label-objekt'
};

//var sectionColor = {

//    1: 'orange',
//    2: 'green',
//    3: 'blue'
//};


app.controller('aggregatedController', ['$rootScope', '$scope', '$filter', '$timeout', '$http', '$sce', 'aggregatedService'
    , function ($rootScope, $scope, $filter, $timeout, $http, $sce, aggregatedService) {

    $scope.storedSearches = [];
    $scope.lastSearchQuery = '';
    $scope.resultList = [];
    $scope.lastResultList = [];
    $scope.FilterGroups = [];
    $scope.applicationsCount = 0;
    $scope.datasetsCount = 0;
    $scope.mapServicesCount = 0;
    $scope.showGeoLocation = false;
    $scope.searchActive = false;
    $scope.initLoad = false;
    $scope.showLoader = false;
    $scope.mapInitialized = false;
    $scope.totalResultCount = 0;
    $scope.filterGroups = {};
    $scope.workList = [];
    $scope.autoCompleteResult = [];
    $scope.lastItems = [];
    $scope.showAllResults = true;
    $scope.selectedSection = { name: 'Filter', hits: 0 };
    $scope.isOpen = false;
    $scope.currentSearchBlockHashCode = '';

    $scope.partials = {
        autoCompleteRow: baseurl + '/Scripts/geonorge/partials/_autoCompleteRow.html',
        storedSearch: baseurl + '/Scripts/geonorge/partials/_storedSearches.html',
        filters: baseurl + '/Scripts/geonorge/partials/_filters.html',
        metadata: baseurl + '/Scripts/geonorge/partials/_metadata.html',
        metadataDetails: baseurl + '/Scripts/geonorge/partials/_metadataDetails.html',
        kartverket: baseurl + '/Scripts/geonorge/partials/_kartverket.html',
        objektkatalog: baseurl + '/Scripts/geonorge/partials/_objektkatalog.html',
        loader: baseurl + '/Scripts/geonorge/partials/_loading.html'
    };

    //To not alter - linked to FileFormatTypes from SiteSeeker
    $scope.fileFormat = {
        0: 'AnyFileFormat',
        1: 'Html',
        2: 'Text',
        3: 'Pdf',
        5: 'Word',
        6: 'Excel',
        7: 'Powerpoint',
        8: 'Rtf',
        20: 'Image'
    };

    $scope.search = function (value, hashCode) {
        
        if (window.sessionStorage) {
            if (value != '') {
                $rootScope.searchQuery = value;
            }
            else if (sessionStorage.searchQuery) {
                $rootScope.searchQuery = sessionStorage.searchQuery; // sessionStorage.getItem('searchQuery'); 
            }
            $scope.lastSearchQuery = angular.copy($rootScope.searchQuery);
            $scope.currentSearchBlockHashCode = hashCode;
            $scope.autoCompleteResult = [];
            $scope.searchActive = true;
            $scope.showLoader = true;
            if (window.sessionStorage) {
                sessionStorage.runSearch = true;
                if (sessionStorage.reloadOnStartup == "true" && hashCode == sessionStorage.getItem('currentSearchBlockHashCode')) {
                    $scope.readFromSessionStorage();
                }

                if (sessionStorage.getItem($scope.currentSearchBlockHashCode)) {
                    $scope.lastFilterGroups = JSON.parse(sessionStorage.getItem($scope.currentSearchBlockHashCode));
                    $scope.FilterGroups = angular.copy($scope.lastFilterGroups);
                }
                else {
                    $scope.lastFilterGroups = [];
                    $scope.FilterGroups = [];
                }
            }
            // opprinnelig: aggregatedService.performSearch($rootScope.searchQuery, [], 0, $rootScope.selectedSearch.section).then(displaySearchData, errorHandler);
            aggregatedService.performSearch($scope.lastSearchQuery, $scope.FilterGroups, 0, 0).then(displayData, errorHandler);                
        }     
    };

    function displaySearchData(response) {
        displayData(response);
    }

    aggregatedService.executeMethod($scope.search);

    $scope.resetFilterGroup = function(group) {
        for (var i = 0; i < group.Filters.length; i++) {
            group.Filters[i].Checked = false;
        }

        filteredSearch();
    };

    $scope.setFilterChecked = function (item) {
        item.Checked = true;
        filteredSearch();
    };
    
    var filteredSearch = function () { 
        $scope.autoCompleteResult = [];
        $scope.showLoader = true;
        // Opprinnelig: aggregatedService.performSearch($scope.lastSearchQuery, $scope.FilterGroups, 0, $rootScope.selectedSearch.section).then(displayData, errorHandler);
        aggregatedService.performSearch($scope.lastSearchQuery, $scope.FilterGroups, 0, 0).then(displayData, errorHandler);
    };

    $scope.resetFilters = function () {
        for (var i = 0; i < $scope.FilterGroups.length; i++) {
            $scope.resetFilterGroup($scope.FilterGroups[i]);
        }

        filteredSearch();
    };

    function displayData(response) {

        $scope.resultList = [];

        if (response.d) {
            applyStyles(response.d);
            if (response.d.RefreshFilters) {
                $scope.FilterGroups = response.d.FilterGroups;
            }
            $scope.NumberOfHitsTotal = response.d.NumberOfHitsTotal;
            $scope.resultList = response.d.Results;
            //for (var x = 0; x < $scope.resultList.length; x++) {
            //    var curr = $scope.resultList[x];
            //    curr.SectionName = sectionTranslations[curr.Section];
            //    curr.className = sectionColor[curr.Section];
            //}
            $scope.selectedSection = { name: 'Filter', hits: 0 };
            for (var i = 0; i < $scope.resultList.length; i++) {
                $scope.resultList[i].visible = true;
            }

            $scope.showAllResults = true;

            $scope.storeToSessionStorage();
            bindLastElementToScroll();
        }
        $scope.initLoad = true;
        $scope.showLoader = false;

        if ($scope.NumberOfHitsTotal > 0) {
            setFilterBoxListener();
        }
    }

    $scope.storeToSessionStorage = function () {
        $scope.lastResultList = angular.copy($scope.resultList);
        $scope.lastFilterGroups = angular.copy($scope.FilterGroups);
        if (window.sessionStorage) {
            sessionStorage.reloadOnStartup = true;
            sessionStorage.filterGroups = angular.toJson($scope.lastFilterGroups);
            sessionStorage.setItem('currentSearchBlockHashCode', $scope.currentSearchBlockHashCode);
            sessionStorage.setItem($scope.currentSearchBlockHashCode, JSON.stringify($scope.lastFilterGroups));
            sessionStorage.resultList = angular.toJson($scope.lastResultList);
            if ($rootScope.searchQuery != '') {
                sessionStorage.searchQuery = $rootScope.searchQuery;
            }
            sessionStorage.lastNumberOfHitsTotal = angular.toJson(angular.copy($scope.NumberOfHitsTotal));
            sessionStorage.showAllResults = angular.copy($scope.showAllResults);
            sessionStorage.selectedSection = angular.toJson(angular.copy($scope.selectedSection));
        }
    };

    $scope.readFromSessionStorage = function () {
        
        if (window.sessionStorage) {

            if (sessionStorage.resultList) {
                $scope.lastResultList = JSON.parse(sessionStorage.resultList);
                $scope.resultList = angular.copy($scope.lastResultList);
            }

            if (sessionStorage.selectedSection) {
                $scope.selectedSection = JSON.parse(sessionStorage.selectedSection);
            }

            if (sessionStorage.lastNumberOfHitsTotal) {
                $scope.NumberOfHitsTotal = angular.copy(JSON.parse(sessionStorage.lastNumberOfHitsTotal));
            }

            if (sessionStorage.showAllResults) {
                $scope.showAllResults = sessionStorage.showAllResults == 'true' ? true : false;
            }

            if ($scope.resultList.length > 0) {
                $scope.initLoad = true;
                $scope.searchActive = true;
                bindLastElementToScroll();
                setFilterBoxListener();
            }            
        }
    };

    $scope.showAllSections = function() {
        $scope.setSectionVisibility(-1);
        $scope.showAllResults = true;
        bindLastElementToScroll();
        $scope.selectedSection = {name: 'Filter', hits: 0};
        $scope.storeToSessionStorage();
    };

    $scope.setSectionVisibility = function (id) {
        
        if (id != -1) $scope.showAllResults = false;

        for (var i = 0; i < $scope.resultList.length; i++) {
            var current = $scope.resultList[i];
            current.visible = current.Section == id || id == -1;
            current.selected = current.Section == id;
            if (current.selected && !$scope.showAllResults) {
                $scope.selectedSection.name = current.SectionName;
                $scope.selectedSection.hits = current.NumberOfHits;
                //$scope.selectedSection.className = sectionColor[current.Section];
            }
        }
        bindLastElementToScroll();
        $scope.storeToSessionStorage();
    };

    $scope.toggleDropdown = function ($event) {
        $event.preventDefault();
        $event.stopPropagation();
        $scope.isOpen = !$scope.isOpen;
    };

    function setFilterBoxListener() {
        var filter = document.getElementById('filterboks');

        window.removeEventListener('scroll', setListener, true);
        window.addEventListener('scroll', setListener, true);

        function setListener() {

            if (filter.offsetHeight > window.innerHeight) {
                filter.removeAttribute('style');
                return;
            }
            var windowBounds = document.body.getBoundingClientRect();
            var resultsHolder = document.getElementById('search-results');
            var resultsBounds = resultsHolder.getBoundingClientRect();

            var width = filter.offsetWidth;
            if (width > 0) {
                if (resultsBounds.top <= 10) {
                    filter.style.width = width + 'px';
                    filter.style.position = 'fixed';
                    filter.style.top = 10 + 'px';
                }
            }

            if (resultsBounds.top >= 10) {
                filter.removeAttribute('style');
            }

            var filterHeight = document.getElementById('filterInner').clientHeight;

            var factor2 = (resultsBounds.top + resultsHolder.offsetHeight) - filterHeight;
            factor2 = factor2 + 10;
            if (factor2 <= 0) {
                filter.removeAttribute('style');
                filter.style.position = 'absolute';
                filter.style.bottom = '0';
            }
        };
    }

    function errorHandler(errorMessage) {
        $scope.resultList = [];
        $scope.FilterGroups = [];
        $scope.NumberOfHitsTotal = 0;
        $scope.initLoad = true;
        $scope.showLoader = false;
    }

    function bindLastElementToScroll() {
        $scope.lastItems = [];
        for (var i = 0; i < $scope.resultList.length; i++) {
            var item = $scope.resultList[i];
            if (item.Results.length > 0 && item.CanGetMoreResults && item.visible) {
                var id = (item.Results.length - 1) + '_' + item.SectionName;

                var newLastItem = {
                    'section': item.Section,
                    'id': id,
                    'disableListener': false,
                    'item': item
                };

                switch (item.Section) {
                    case sections.MetadataID:
                    //case sections.ObjectCatalogueID:
                    //    newLastItem.NextRecordNumber = item.NextRecordNumber;
                    //    break;
                    case sections.GeonorgeID:
                        newLastItem.NextPaginationNumber = item.NextPaginationNumber;
                        break;
                }
                $scope.lastItems.push(newLastItem);

            }
        }

        window.removeEventListener('scroll', scrollHandler, true);
        window.addEventListener('scroll', scrollHandler, true);
    }

    var scrollHandler = function (ev) {
        var winHeight = window.innerHeight;

        for (var x = 0; x < $scope.lastItems.length; x++) {
            var curr = $scope.lastItems[x];
            var element = document.getElementById(curr.id);
            if (element) {
                var bounds = element.getBoundingClientRect();
                var top = bounds.top;
                var triggerPoint = top - winHeight;

                if (triggerPoint < 0 && !curr.disableListener) {
                    switch (curr.section) {
                        case sections.MetadataID:
                        //case sections.ObjectCatalogueID:
                            aggregatedService.getNextResultsCommon($scope.lastSearchQuery, $scope.FilterGroups, curr.item).then(addRowsToMetadataList, errorHandler);
                            curr.disableListener = true;
                            break;
                        case sections.GeonorgeID:
                            aggregatedService.getNextResultsGeonorge($scope.lastSearchQuery, $scope.FilterGroups, curr.NextPaginationNumber, curr.item).then(addRowsToGeonorgeList, errorHandler);
                            curr.disableListener = true;
                            break;
                    }
                }
            }
        }
    };

    function addRowsToMetadataList(response) {
        var curr = {};
        if (response.d) {
            for (var i = 0; i < $scope.resultList.length; i++) {
                curr = $scope.resultList[i];
                if ($scope.resultList[i].Section == response.d.Section) {
                    curr.Results = curr.Results.concat(response.d.Results);
                    curr.NextRecordNumber = response.d.NextRecordNumber;
                    curr.CanGetMoreResults = response.d.CanGetMoreResults;
                    curr.disableListener = false;
                    bindLastElementToScroll();
                    break;
                }
            }
            curr.showLoader = false;
            $scope.storeToSessionStorage();
        }
    }

    function addRowsToGeonorgeList(response) {
        var curr = {};
        if (response.d) {
            for (var i = 0; i < $scope.resultList.length; i++) {
                curr = $scope.resultList[i];
                if ($scope.resultList[i].Section == response.d.Section) {
                    curr.Results = curr.Results.concat(response.d.Results);
                    curr.NextPaginationNumber = response.d.NextPaginationNumber;
                    curr.CanGetMoreResults = response.d.CanGetMoreResults;
                    curr.disableListener = false;
                    bindLastElementToScroll();
                    break;
                }
            }
            curr.showLoader = false;
            $scope.storeToSessionStorage();
        }
    }

    function applyStyles(list) {
        for (var i = 0; i < list.FilterGroups.length; i++) {
            list.FilterGroups[i].label = scopeFilterLabel[list.FilterGroups[i].GroupId];
            list.FilterGroups[i].icon = scopeFilterIcons[list.FilterGroups[i].GroupId];
        }
    }

    $scope.getResultTemplate = function (type) {

        switch (type) {
            case sections.GeonorgeID:
                return $scope.partials.kartverket;
            case sections.MetadataID:
                return $scope.partials.metadata;
            //case sections.ObjectCatalogueID:
            //    return $scope.partials.objektkatalog;
            default:
                return null;
        }

    };

    $scope.checkImage = function checkImage(src) {
    
        var img = new Image(src), re = "false";
        img.onload = function () {
            re = "true"
            return re
        };
        return re;

    };

    $scope.trustHtml = function (html) {
        return $sce.trustAsHtml(html);
    };

    $scope.getStatusClass = function (status) {
        if (status == null)
            return null;
        switch (status.toLowerCase()) {
            case 'vedtatt':
                return 'vedtatt';
            case 'under arbeid':
                return 'underarbeid';
            case 'godkjent':
                return 'godkjent';
            default:
                return null;
        }
    };

    // Gammel kode, erstattet med koden over der label- er tatt bort. Denne kan vel fjernes?
    //$scope.getStatusClass = function (status) {
    //    if (status == null)
    //        return null;
    //    switch (status.toLowerCase()) {
    //        case 'vedtatt':
    //            return 'label-vedtatt';
    //        case 'under arbeid':
    //            return 'label-underarbeid';
    //        case 'godkjent':
    //            return 'label-godkjent';

    //        default:
    //            return null;
    //    }
    //};

    $scope.redirect = function (url, newWindow) {
        if (newWindow) window.open(url, '_blank');
        else window.location = url;
    };


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
    }]);
}());


