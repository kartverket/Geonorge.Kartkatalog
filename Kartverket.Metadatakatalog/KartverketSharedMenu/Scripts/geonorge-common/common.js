$(document).ready(function () {
    //var cbName = "cb-enabled";

    //if ($.cookie(cbName) && $.cookie(cbName) === 'accepted') {
    //    $(".cookie").hide();
    //}

    //$(".cookie button").on("click", function () {
    //    $(".cookie").hide();
    //    $.cookie(cbName, 'accepted', { expires: 10, path: '/' });
    //});

//    $(".editorialblock .page-wizard").parents("[class^='col']").addClass("page-wizard-wrapper");


    $(".page-wizard .page-wizard-tab").on("click", function () {
        $(".page-wizard").toggleClass("open");
    });

    var liUl = $("<li class='jose'><ul></ul></li>");

    $("<p>te</p>").appendTo($(liUl).find("ul"));
    
    


//    var menuLi = $(".dropdown-menu > li");
//    $.each(menuLi, function (k, v) {
//        var li = $(this);
//
////        li.appendTo(liUl);
//        if (k % 8 === 0) {
//            li.appendTo($(liUl).find("ul"));
//
////            console.log($(v).parentsUntil(".dropdown-menu"));
//
////            liUl.appendTo(li.parents(".dropdown-menu"));
//
//        }
//
//    });
//
//    console.log(liUl[0]);
//    $(".dropdown-menu > li").addClass('col-md-4');
});