$(document).ready(function () {
    $(".page-wizard .page-wizard-tab").on("click", function () {
        $(".page-wizard").toggleClass("open");
    });

    var liUl = $("<li class='jose'><ul></ul></li>");

    $("<p>te</p>").appendTo($(liUl).find("ul"));
});