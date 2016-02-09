function display(selector, data) {
    if (data && typeof data === 'string') {
        data = JSON.parse(data);
    }
    if (data) {
        data = JSON.stringify(data, null, 2);
    }

    $(selector).text(data);
}

var settings = {
    authority: 'https://localhost:44300/identity',
    client_id: 'js',
    popup_redirect_uri: 'https://localhost:44300/JsClient/Popup',

    response_type: 'id_token',
    scope: 'openid profile',

    filter_protocol_claims: true
};

var manager = new OidcTokenManager(settings);

$('.js-login').click(function () {
    manager.openPopupForTokenAsync()
        .then(function () {
            display('.js-id-token', manager.profile);
        }, function (error) {
            console.error(error);
        });
});