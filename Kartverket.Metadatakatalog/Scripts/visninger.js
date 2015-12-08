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


function orderBy() {
    var orderbyArray = getURLParameter("orderby").split("_");
    var orderHeaders = $("#tableView .search-results-table-heading .orderby");
    if (orderbyArray != "") {
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




/*
function SortBy(sort) {
    var sort = document.getElementById("sorting");
    var selected = sort.options[sort.selectedIndex].text;
    localStorage.setItem("sortering", selected);
    document.sortering.submit();
}*/


// Loading animation
/*
$(window).load(function () {
    $('#loading').hide();
});
*/

