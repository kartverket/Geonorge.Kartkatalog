function listView() {
    $(".table-heading").remove();

    // Buttons   
    $('#button-listView').addClass('active');
    $('#button-galleryView').removeClass('active');
    $('#button-tableView').removeClass('active');

    $('.search-results').removeClass('table-view');
    $('.search-results').removeClass('gallery-view');
    $('.search-results').addClass('list-view');

    localStorage.setItem("visningstype", "liste");

}

function galleryView() {
    $(".table-heading").remove();

    // Buttons
    $('#button-listView').removeClass('active');
    $('#button-galleryView').addClass('active');
    $('#button-tableView').removeClass('active');

    $('.search-results').removeClass('table-view');
    $('.search-results').removeClass('list-view');
    $('.search-results').addClass('gallery-view');

    localStorage.setItem("visningstype", "galleri");

}



function tableView() {
    $(".table-heading").remove();
    $('.search-results').prepend("<div class='clearfix'></div><div class='col-xs-12 table-heading'><div class='col-xs-9'><div class='col-xs-4'><h4>Tittel</h4></div><div class='col-xs-4'><h4>Eier</h4></div><div class='col-xs-4'><h4>Type</h4></div></div><div class='col-xs-3'><div class='col-sm-3'><h4></h4></div><div class='col-xs-3'><h4></h4></div><div class='col-xs-3'><h4></h4></div><div class='col-xs-3'><h4></h4></div></div></div>");

    // Buttons
    $('#button-listView').removeClass('active');
    $('#button-galleryView').removeClass('active');
    $('#button-tableView').addClass('active');

    $('.search-results').removeClass('gallery-view');
    $('.search-results').removeClass('list-view');
    $('.search-results').addClass('table-view');



    $(window).scroll(function () {
        if ($(window).width() >= 992) {
            if ($(window).scrollTop() > 327) {
                $(".table-heading").css({ "top": ($(window).scrollTop()) - 327 + "px" });
                $(".table-heading").css("background-color", "white");
                $(".table-heading").css("z-index", "400");
            } else {
                $(".table-heading").css("top", "0");
            }
        } else if ($(window).width() < 992 && $(window).width() >= 750) {
            if ($(window).scrollTop() > 345) {
                $(".table-heading").css({ "top": ($(window).scrollTop()) - 345 + "px" });
                $(".table-heading").css("background-color", "white");
                $(".table-heading").css("z-index", "400")
            } else {
                $(".table-heading").css("top", "0");
            }
        } else if ($(window).width() < 750) {
            if ($(window).scrollTop() > 365) {
                $(".table-heading").css({ "top": ($(window).scrollTop()) - 365 + "px" });
                $(".table-heading").css("background-color", "white");
                $(".table-heading").css("z-index", "400")
            } else {
                $(".table-heading").css("top", "0");
            }
        } else {
            $(".table-heading").css("top", "0");
        }
    });

    localStorage.setItem("visningstype", "tabell");

}

function SortBy(sort) {
    var sort = document.getElementById("sorting");
    var selected = sort.options[sort.selectedIndex].text;
    localStorage.setItem("sortering", selected);
    document.sortering.submit();

}

$(document).ready(function () {
    var visningstype = localStorage.getItem("visningstype");

    if (visningstype == "galleri") { galleryView() }
    if (visningstype == "liste") { listView() }
    if (visningstype == "tabell") {
        // Listevisning ved liten skjerm
        if ($(window).width() < 600) {
            listView();
        } else {
            tableView()
        }
    }
});


// Loading animation
/*
$(window).load(function () {
    $('#loading').hide();
});
*/

