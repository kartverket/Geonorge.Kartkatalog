$(function () {
    $(".layout-block-close-main-content-area[href^=\"#\"]").click(function (event) {
        var target = $($(this).attr("href"));

        if (target.length) {
            event.preventDefault();

            $(".main-content-area", target).collapse("toggle");

            $("html, body").animate({
                scrollTop: target.offset().top
            }, 1000);
        }
    });

    $(".to-top").click(function () {
        $("html, body").animate({
            scrollTop: 0
        }, 1000);
    });

    $(".standard-page-main-content-area .formblock form tr").each(function () {
        $(this).find("td").each(function () {
            if ($(this).children().length === 0) {
                $(this).addClass("hidden");
            }

            if ($(this).find("input[type=\"submit\"]").length) {
                $(this).closest("tr").addClass("submit");
            }
        });
    });
});

function getServiceStatus(uuid, id) {
    $.getJSON("https://status.geonorge.no/monitorApi/serviceDetail?uuid=" + uuid, function (result) {
        console.log(result);
        try {
            var numLayers = parseInt(result.details[12][1]);
            console.log("numLayers:" + numLayers);
            if (numLayers > 30) {
                $('#mapmacro-' + id).attr("class", "custom-icon custom-icon-kartmarkoer-warning");
                $('#mapmacrolink-' + id).attr("title", "Tjenesten kan være treg å vise");
            }
        }
        catch (err) {
            console.log(err);
        }
    });
};