function getURLParameter(name) {
    if (location.search != "" && location.search != null) {
        return decodeURIComponent((new RegExp('[?|&]' + name + '=' + '([^&;]+?)(&|#|;|$)').exec(location.search) || [, ""])[1].replace(/\+/g, '%20')) || null
    } else {
        return "";
    }
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

$(document).ready(function () {
    var layout = "tableView";
    if (localStorage.getItem("layout") != null) {
        layout = localStorage.getItem("layout");
    };
    orderBy();
    changeLayout(layout);
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
