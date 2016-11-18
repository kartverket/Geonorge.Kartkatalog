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
        console.log(result);
        try {
            var numLayers = parseInt(result.numLayers.svar);
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
};function addToCartButtonClick(addToCartButton) {
    var added = false;
    var itemuuid = addToCartButton.attr('itemuuid');
    var itemname = addToCartButton.attr('itemname');
    var itemurl = addToCartButton.attr('itemurl');
    var itemorglogo = addToCartButton.attr('itemorglogo');
    var itemdisturl = addToCartButton.attr('itemdisturl');
    var itemtheme = addToCartButton.attr('itemtheme');
    var itemorgname = addToCartButton.attr('itemorgname');

    updateCartButton(addToCartButton);


    if (localStorage.getItem('orderItems') != null) {
        orderItems = (JSON.parse(localStorage.getItem('orderItems')));
    }
    $.map(orderItems, function (elementOfArray, indexInArray) {
        if (elementOfArray == itemuuid) {
            orderItems.push(itemuuid);
            added = true;
        }
    });
    if (!added) {
        orderItems.push(itemuuid);
        var metadata = { 'name': itemname, 'uuid': itemuuid, 'url': itemurl, 'organizationLogoUrl': itemorglogo, 'distributionUrl': itemdisturl, 'theme': itemtheme, 'organizationName': itemorgname };
        localStorage["orderItems"] = JSON.stringify(orderItems);
        localStorage[itemuuid + ".metadata"] = JSON.stringify(metadata);
        showAlert(itemname + ' er lagt til i <a href="/Download">kurven</a>', 'success');

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
        showAlert(itemname + ' er allerede lagt til i <a href="/Download">kurven</a>', 'warning');
    }
}