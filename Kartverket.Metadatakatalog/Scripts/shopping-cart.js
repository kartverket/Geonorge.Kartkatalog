const getCookie = function(cname) {
    const name = `${cname}=`;
    const decodedCookie = decodeURIComponent(document.cookie);
    const ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

const setCookie = function(cname, cvalue, exdays) {
    let expireDate = new Date();
    expireDate.setTime(expireDate.getTime() + (exdays * 24 * 60 * 60 * 1000));
    const expires = `expires=${expireDate.toUTCString()}`;
    const domain = window.location.hostname === 'localhost' ? '' : 'domain=.geonorge.no';
    document.cookie = `${cname}=${cvalue};${expires};path=/;${domain}`;
}


// Override for felleskomponenter
updateShoppingCart = function() {
    let orderItems = "";
    let orderItemsObj = {};
    const cookieName = "orderItems";
    let cookieValue = getCookie(cookieName) || 0;

    if (localStorage.getItem("orderItems") != null) {
        orderItems = localStorage.getItem("orderItems");
    }
    if (orderItems != "") {
        orderItemsObj = JSON.parse(orderItems);
        cookieValue = orderItemsObj.length; 
    }
    setCookie(cookieName, cookieValue, 7);
}


// Override for felleskomponenter
updateShoppingCartCookie = function() {
    const cookieName = "orderItems";
    let cookieValue = 0;

    if (localStorage.getItem("orderItems") != null && localStorage.getItem("orderItems") != "[]") {
        const orderItems = localStorage.getItem("orderItems");
        const orderItemsObj = JSON.parse(orderItems);
        cookieValue = orderItemsObj.length;
    }
    setCookie(cookieName, cookieValue, 7);
}

