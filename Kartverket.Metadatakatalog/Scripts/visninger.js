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

$(document).ready(function () {
    var layout = "tableView";
    if (localStorage.getItem("layout") != null) {
        layout = localStorage.getItem("layout");
    };
    changeLayout(layout);
});


function additionalView(buttonId) {
    $("#saveButtons a").attr("class", "hidden");
    $("#" + buttonId).attr("class", "btn");
}


function SortBy(sort) {
    var sort = document.getElementById("sorting");
    var selected = sort.options[sort.selectedIndex].text;
    localStorage.setItem("sortering", selected);
    document.sortering.submit();

}


// Loading animation
/*
$(window).load(function () {
    $('#loading').hide();
});
*/

