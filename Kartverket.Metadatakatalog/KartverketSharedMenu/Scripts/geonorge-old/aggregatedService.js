var app = angular.module('geonorge');
app.service('aggregatedService', ['$http', '$q', function ($http, $q) {
    var txtLang = document.getElementById('txtLang');
    var lang = '';
    if (txtLang) lang = txtLang.value;

    var methodToExecute = undefined;

    return ({
        triggerSearch: triggerSearch,
        executeMethod : executeMethod,
        performSearch: performSearch,
        getNextResultsGeonorge: getNextResultsGeonorge,
        getNextResultsMetadata: getNextResultsMetadata,
        getNextResultsCommon: getNextResultsCommon
    });

    

    function executeMethod(method) {
        methodToExecute = method;
    }

    function triggerSearch(value) {
        return $q(function(reject)
        {
            if (methodToExecute == undefined) {
                reject();
            } else {
                methodToExecute(value);
            }
        });
    }

    function performSearch(query, filters, limit, section) {
        if (typeof (limit) === 'undefined') limit = 0;
        //Facets for siteseeker search
        var facets = getFacetsForGeonorge(filters, sections.GeonorgeID);

        var apiService = baseurl + '/ws/geonorge.asmx/GetAggregatedSearch';
        var request = $http({
            method: 'POST',
            url: apiService,
            params: {
                'query': query,
                'fv': facets,
                'language': lang
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8',
                'accept': '*/*'
            },
            data: { 'filterGroups': filters, 'limit': limit, 'section': section }
        });

        return (request.then(handleSuccess, handleError));
    }

    function getNextResultsGeonorge(query, filters, paginationNumber, item) {
        item.showLoader = true;
        //Facets for siteseeker search
        var facets = getFacetsForGeonorge(filters, sections.GeonorgeID);
        var params = buildParamsGeonorge(query, facets, paginationNumber);
        var apiService = baseurl + '/ws/geonorge.asmx/GetNextResultsGeonorge';
        var request = $http({
            method: 'POST',
            url: apiService,
            params: params,
            headers: {
                'Content-Type': 'application/json; charset=utf-8',
                'accept': '*/*'
            },
            data: {}
        });

        return (request.then(handleSuccess, handleError));
    }

    function buildParamsGeonorge(query, facets, nextPaginationNumber) {
        var params = {
            'query': query == null ? "" : query,
            'fv': facets,
            'pn': nextPaginationNumber,
            'language': lang
        };

        return params;
    }

    function getFacetsForGeonorge(groups, section) {
        var facetList = [];
        for (var i = 0; i < groups.length; i++) {
            var group = groups[i];
            for (var x = 0; x < group.Filters.length; x++) {
                var filter = group.Filters[x];
                if (filter.ParentSections.indexOf(section) != -1) {
                    if (filter.Checked) {
                        facetList.push(filter.Id);
                    }
                }
            }
        }

        return facetList.toString();
    }

    function getNextResultsMetadata(query, filterGroups, nextRecord, item) {}

    function getNextResultsCommon(query, filterGroups, item) {
        item.showLoader = true;
        var data = buildDataMetadata(filterGroups, query, item);
        var apiService = baseurl + '/ws/geonorge.asmx/GetNextResultsCommon';
        var request = $http({
            method: 'POST',
            url: apiService,
            params: { 'language': lang },
            headers: {
                'Content-Type': 'application/json; charset=utf-8',
                'accept': '*/*'
            },
            data: data
        });

        return (request.then(handleSuccess, handleError));
    }

    function buildDataMetadata(filterGroups, query, item) {
        var params = {
            'filterGroups': filterGroups,
            'section': item.Section,
            'startNumber': item.NextRecordNumber,
            'query': query == null ? "" : query
        };

        return params;
    }

    function handleSuccess(response) {
        return (response.data);
    }

    function handleError(response) {
        if (!angular.isObject(response.data) ||
            !response.data.message) {

            return ($q.reject("An unknown error occurred."));

        }
        return ($q.reject(response.data.message));
    }


}]);