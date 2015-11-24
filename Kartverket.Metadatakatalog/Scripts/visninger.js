function changeLayout(layout) {
    $(".search-results").attr("id", layout);
    localStorage.setItem("layout", layout);
}

$(document).ready(function () {
    var layout = localStorage.getItem("layout");
    changeLayout(layout);
});


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

