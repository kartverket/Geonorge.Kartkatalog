function getURLParameter(name) {
    if (location.search != "" && location.search != null) {
        return decodeURIComponent((new RegExp('[?|&]' + name + '=' + '([^&;]+?)(&|#|;|$)').exec(location.search) || [, ""])[1].replace(/\+/g, '%20')) || null
    } else {
        return "";
    }
}


function getAllUrlParams(url) {

    // get query string from url (optional) or window
    var queryString = url ? url.split('?')[1] : window.location.search.slice(1);

    // store the parameters
    var obj = {};

    // if query string exists
    if (queryString) {

        queryString = queryString.split('#')[0];

        var arr = queryString.split('&');

        for (var i = 0; i < arr.length; i++) {
            var a = arr[i].split('=');

            var paramNum = undefined;
            var paramName = a[0].replace(/\[\d*\]/, function (v) {
                paramNum = v.slice(1, -1);
                return '';
            });

            var paramValue = typeof (a[1]) === 'undefined' ? true : a[1];

            if (obj[paramName]) {
                if (typeof obj[paramName] === 'string') {
                    obj[paramName] = [obj[paramName]];
                }
                if (typeof paramNum === 'undefined') {
                    obj[paramName].push(paramValue);
                }
                else {
                    obj[paramName][paramNum] = paramValue;
                }
            }
                // if param name doesn't exist yet, set it
            else {
                obj[paramName] = paramValue;
            }
        }
    }

    return obj;
}


function changeLayout(layout) {
    $(".search-results").attr("id", layout);
    var options = $("#layoutSelect option");
    var selectedOption = $("#layoutSelect option[value='" + layout + "']");
    $.each(options, function () {
        if ($(this).attr("value") != layout) {
            $(this).attr("selected", false);
        } else {
            $(this).attr("selected", true)
        }
    });
    localStorage.setItem("layout", layout);
}

function changeLayoutWithoutLocalStorage(layout) {
    $(".search-results").attr("id", layout);
}

function orderBy() {
    var orderbyArray = getURLParameter("orderby");
    var orderHeaders = $("#tableView .search-results-table-heading .orderby");
    if (orderbyArray != "" && orderbyArray != null) {
        orderbyArray = orderbyArray.split("_");
        var orderby = orderbyArray[0];
        var direction = "asc";
        if (orderbyArray.length > 1) {
            direction = orderbyArray[1];
        }

        $.each(orderHeaders, function () {
            if ($(this).hasClass("orderby-" + orderby)) {
                if ($(this).hasClass("orderby-" + direction)) {
                    $(this).hide();
                } else {
                    $(this).show();
                    $(this).addClass("active-orderby");
                }
            } else if ($(this).hasClass("orderby-asc")) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    } else {
        $("#tableView .orderby-desc").hide();
    }
}

function getFacetFromUrl() {
    var activeFilters = {}
    if (JSON.parse(localStorage.getItem("activeFilters")) != null) {
        activeFilters = JSON.parse(localStorage.getItem("activeFilters"));
    };
    var regExp = new RegExp(".*Facets\\[0\\]\.name=([a-z]*)&.*");
    var searchString = decodeURIComponent(location.search);
    var facetName = regExp.exec(searchString);
    if (facetName != null) {
        $.each(facetName, function (key, value) {
            if (value == 'theme') {
                activeFilters['#theme'] = true;
            }
        });
    }
    var activeFiltersJSON = JSON.stringify(activeFilters);
    localStorage.setItem("activeFilters", activeFiltersJSON);

};

function createFacetUrlParameter(index, name, value, trailingAmpersand) {
    var ampersand = '';
    if (trailingAmpersand !== undefined && trailingAmpersand == true) ampersand = '&';

    var facetUrlParameter = 'Facets%5B' + index + '%5D.name=' + name + '&Facets%5B' + index + '%5D.value=' + value + ampersand;

    return facetUrlParameter;
}

function createStringUrlParameter(name, value, trailingAmpersand) {
    var ampersand = '';
    if (trailingAmpersand !== undefined && trailingAmpersand == true) ampersand = '&';

    var stringUrlParameter = name + '=' + value + ampersand;

    return stringUrlParameter;

}

function getSelectedFacets() {
    var allUrlParameters = getAllUrlParams(location.search);
    var allFacets = {};
    // get all facets
    for (var parameter in allUrlParameters) {
        if (allUrlParameters.hasOwnProperty(parameter)) {
            var res = parameter.split(".");
            var facet = allUrlParameters[parameter];
            if (res.length > 1) {
                var facet = allFacets[res[0]] !== undefined ? allFacets[res[0]] : {};
                facet[res[1]] = allUrlParameters[parameter];
            }
            allFacets[res[0]] = facet;

        }
    }
    return allFacets;
}

function getAllUrlParamsForSelectedFacets(selectedFacets) {
    var allFacets = selectedFacets !== undefined ? selectedFacets : getSelectedFacets();
    var allUrlParameters = { parameterString: '?', parameterLength: 0 };
    var facetIndex = 0;
    for (var key in allFacets) {
        if (allFacets.hasOwnProperty(key)) {
            var facet = allFacets[key];
            var urlParameter = '';
            if (typeof (facet) == "string") {
                urlParameter = createStringUrlParameter(key, facet, true);
                allUrlParameters.parameterString += urlParameter;
            } else if (facet.value !== undefined) {
                urlParameter = createFacetUrlParameter(allUrlParameters.parameterLength, facet.name, facet.value, true);
                allUrlParameters.parameterString += urlParameter;
                allUrlParameters.parameterLength++;
            }
        }
    }

    return allUrlParameters;
}

function getUrlForFacetLink(facetLinkElement) {
    var facetName = $(facetLinkElement).data('facet-name');
    var facetValue = $(facetLinkElement).data('facet-value');
    var selectedFacets = getSelectedFacets();
    var alreadySelected = false;

    for (var key in selectedFacets) {
        if (selectedFacets.hasOwnProperty(key)) {
            var facet = selectedFacets[key];
            // Remove already selected facet
            if (facet.name == escape(facetName) && facet.value == escape(facetValue)) {
                delete selectedFacets[key];
                alreadySelected = true;
            }
        }
    }

    var newUrlParameter = '';
    var urlParameters = getAllUrlParamsForSelectedFacets(selectedFacets);
    if (!alreadySelected) newUrlParameter = createFacetUrlParameter(urlParameters.parameterLength, facetName, facetValue, false);
    else urlParameters.parameterString = urlParameters.parameterString.slice(0, -1); // Remove last amperstand

    var mergedUrlParameters = urlParameters.parameterString + newUrlParameter;

    var url = window.location.origin + window.location.pathname + mergedUrlParameters;

    return url;
}


$(document).ready(function () {
    var layout = "tableView";
    if (localStorage.getItem("layout") != null) {
        layout = localStorage.getItem("layout");
    };
    orderBy();
    changeLayout(layout);

    $('.facet-link').on('click', function (event) {
        event.preventDefault();
        var facetLinkUrl = getUrlForFacetLink($(this));

        window.location.href = facetLinkUrl;
    })
});


function additionalView(buttonId) {
    $("#saveButtons a").attr("class", "hidden");
    $("#" + buttonId).attr("class", "btn");
}

function triggerMobileLayout() {
    var windowWidth = $(window).width();
    if (windowWidth < 751) {
        changeLayoutWithoutLocalStorage("listView");
    } else {
        var layout = localStorage.getItem("layout");
        changeLayoutWithoutLocalStorage(layout);
    }
}

window.onresize = function (event) {
    triggerMobileLayout();
}
