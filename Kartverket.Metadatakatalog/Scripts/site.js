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
    $.getJSON(statusApi + "monitorApi/serviceDetail?uuid=" + uuid, function (result) {
        try {
            var vurderingIsDefined = result.connect !== undefined && result.connect.vurdering !== undefined;
            var numLayersIsDefined = result.numLayers !== undefined && result.numLayers.svar !== undefined;
            var statusOK =  vurderingIsDefined && result.connect.vurdering != "no";
            var numLayers = parseInt(numLayersIsDefined ? result.numLayers.svar : 0);
            if (!statusOK) {
                $('#mapmacro-' + id + ', #mapmacro-button-' + id).attr("class", "custom-icon custom-icon-kartmarkoer-unavailable");
                $('#mapmacrolink-' + id + ', #mapmacrolink-button-' + id).attr("title", "Tjenesten er utilgjengelig for øyeblikket");
                $('#mapmacrolink-' + id + ', #mapmacrolink-button-' + id).removeAttr("href");
                $('#mapmacrolink-' + id + ', #mapmacrolink-button-' + id).removeAttr("onclick");
                $('#mapmacrolink-' + id + ', #mapmacrolink-button-' + id).attr("disabled", "disabled");
                $('#mapmacrolink-' + id + ', #mapmacrolink-button-' + id).addClass("disabled");
            }
            else if (numLayers > 30) {
                $('#mapmacro-' + id + ', #mapmacro-button-' + id).attr("class", "custom-icon custom-icon-kartmarkoer-warning");
                $('#mapmacrolink-' + id + ', #mapmacrolink-button-' + id).attr("title", "Tjenesten kan være treg å vise");
            }
        }
        catch (err) {
            console.log(err);
        }
    });
};



function addToCartButtonClick(addToCartButton) {
    var added = false;
    var itemuuid = addToCartButton.attr('itemuuid');
    var itemname = addToCartButton.attr('itemname');
    var itemurl = addToCartButton.attr('itemurl');
    var itemorglogo = addToCartButton.attr('itemorglogo');
    var itemdisturl = addToCartButton.attr('itemdisturl');
    var itemtheme = addToCartButton.attr('itemtheme');
    var itemorgname = addToCartButton.attr('itemorgname');


    var orderItems = [];

    if (localStorage.getItem('orderItems') != null) {
        orderItems = (JSON.parse(localStorage.getItem('orderItems')));
    }
    $.map(orderItems, function (elementOfArray, indexInArray) {
        if (elementOfArray == itemuuid) {
            added = true;
        }
    });
    if (!added) {
        orderItems.push(itemuuid);
        var metadata = { 'name': itemname, 'uuid': itemuuid, 'url': itemurl, 'organizationLogoUrl': itemorglogo, 'distributionUrl': itemdisturl, 'theme': itemtheme, 'organizationName': itemorgname };
        localStorage["orderItems"] = JSON.stringify(orderItems);
        localStorage[itemuuid + ".metadata"] = JSON.stringify(metadata);
        showAlert(itemname + ' er lagt til i <a href="/nedlasting">kurven</a>', 'success');

        var orderItemCount = $('#orderitem-count').text();
        if (orderItemCount == null || orderItemCount == '') {
            orderItemCount = 0;
            $('#orderitem-count-text').text(' datasett');
        } else {
            orderItemCount = parseInt($('#orderitem-count').text());
        }
        orderItemCount += 1;
        $('#orderitem-count').text(orderItemCount);
        updateShoppingCart();

    } else {
        var index = orderItems.indexOf(itemuuid);
        if (index > -1) {
            orderItems.splice(index, 1);
        }
        localStorage.removeItem(itemuuid + ".metadata");
        localStorage["orderItems"] = JSON.stringify(orderItems);

        updateShoppingCart();

        showAlert(itemname + ' er fjernet fra <a href="/nedlasting">kurven</a>', 'warning');
    }

    updateCartButton(addToCartButton, orderItems);

}

$(document).ready(function () {
    var addToCartId = getParameterByName('addtocart_event_id');
    if (addToCartId) {
        var addToCartButton = $("#" + addToCartId);
        addToCartButtonClick(addToCartButton);
    }
})
