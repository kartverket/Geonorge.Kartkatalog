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
