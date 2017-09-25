function removeSingleItemFromArray(array, undesirableItem) {
    var index = array.indexOf(undesirableItem);
    if (index >= 0) {
        array.splice(index, 1);
    }
    return array;
}

function removeFromArray(array, undesirableItems) {
    var multiple = Array.isArray(undesirableItems);
    if (multiple) {
        undesirableItems.forEach(function (undesirableItem) {
            array = removeSingleItemFromArray(array, undesirableItem);
        });
    } else {
        array = removeSingleItemFromArray(array, undesirableItems);
    }
}

function orderItemHasMetadata(orderItemUuid) {
    var orderItemHasMetadata = localStorage.getItem(orderItemUuid + ".metadata") != null ? true : false;
    return orderItemHasMetadata;
}

function removeBrokenOrderItems() {
    var orderItems = JSON.parse(localStorage.getItem("orderItems"));
    if (orderItems != null) {
        removeFromArray(orderItems, [null, undefined, "null", {}, ""]);
        orderItems.forEach(function (orderItem) {
            if (!orderItemHasMetadata(orderItem)) {
                removeSingleItemFromArray(orderItems, orderItem);
            }
        });
        localStorage.setItem('orderItems', JSON.stringify(orderItems));
    }
}

function updateShoppingCart() {
    var shoppingCartElement = $('#orderitem-count');
    var orderItems = "";
    var orderItemsObj = {};
    var cookieName = "orderitems";
    var cookieValue = 0;
    var cookieDomain = ".geonorge.no";

    if (localStorage.getItem("orderItems") != null) {
        orderItems = localStorage.getItem("orderItems");
    }

    if (orderItems != "") {
        orderItemsObj = JSON.parse(orderItems);
        cookieValue = orderItemsObj.length;
        if (cookieValue > 0) {
            shoppingCartElement.css("display", "block");
            shoppingCartElement.html(cookieValue);
            addShoppingCartTooltip(cookieValue);
        } else {
            shoppingCartElement.css("display", "none");
            addShoppingCartTooltip(0);
        }

    } else if ($.cookie(cookieName) !== undefined && $.cookie(cookieName) != 0 && $.cookie(cookieName) != null) {
        cookieValue = $.cookie(cookieName);
        shoppingCartElement.css("display", "block");
        shoppingCartElement.html(cookieValue);
    } else {
        shoppingCartElement.css("display", "none");
    }
    $.cookie(cookieName, cookieValue, { expires: 7, path: '/', domain: cookieDomain });
}

function updateShoppingCartCookie() {
    var shoppingCartElement = $('#orderitem-count');
    var cookieName = "orderitems";
    var cookieDomain = ".geonorge.no";
    var cookieValue = 0;
    if (localStorage.getItem("orderItems") != null && localStorage.getItem("orderItems") != "[]") {
        var orderItems = localStorage.getItem("orderItems");
        var orderItemsObj = JSON.parse(orderItems);
        cookieValue = orderItemsObj.length;
        shoppingCartElement.html(cookieValue);
    } else {
        cookieValue = 0;
        shoppingCartElement.css("display", "none");
    }
    $.cookie(cookieName, cookieValue, { expires: 7, path: '/', domain: cookieDomain });
}


$(window).load(function () {
    removeBrokenOrderItems();
    updateShoppingCart();
});
