$(function() {
    var adminLink = $('#cms-admin-link');

    adminLink.click(function() {
        showLogin();
        return false;
    });

    if (location.hash == "#login-fail") {
        adminLink.click();
        alert('Login failed.');
    }

    if (window.cms.isNewUser) {
        processNewUser();
    }
});

function processNewUser() {
    function okClicked() {
        var dialog = $(this);
        var name = dialog.find('input[name=name]').val();
        var token = dialog.find('input[name=token]').val();
        $.ajax({
            type: 'post',
            url: cms.urlBase + '_auth/addadmin',
            data: { name: name, token: token },
            success: function() {
                dialog.dialog('close');
                $('<div title="Thanks">You can now edit this website.</div>').dialog({
                    modal: true,
                    buttons: { 'OK': function() { location.reload(true); } }
                });
            },
            error: function(xhr) {
                alert(xhr.responseText);
            }
        });
    }

    $('<div title="Hello new user!">\
          <p>To become an admin for this website, please fill in this form.</p>\
          <fieldset>\
            <label>Your Name</label>\
            <input type="text" name="name" style="width: 250px"/>\
            <label>Sign-up Number</label>\
            <input type="text" name="token" style="width: 250px"/>\
          </fieldset>\
       </div>').dialog({ modal: true, buttons: { 'OK': okClicked} });
}

function showLogin() {
    if ($('#login-dialog').length == 0) {
        $('body').append(
            '<div id="login-dialog">\
                <form method="post" action="' + window.cms.urlBase + '_auth/login" style="margin: 20px 0">\
                    <label>OpenID:</label>\
                    <input type="text" name="id" style="width: 380px"/>\
                    <input type="submit" value="Log In"/>\
                </form>\
                <p id="login-google" style="font-size: 0.8em"><a href="#">Log in with Google</a></p>\
             </div>');
        $('#login-dialog')
            .dialog({
                title: 'Website Admin Login',
                width: 600,
                modal: true,
                autoOpen: false
            })
            .find('form').submit(function() {
                $('#login-dialog input[type=submit]').attr('disabled', 'disabled').val('Please wait');
                var url = $(this).attr('action');
                $.ajax({
                    type: 'post',
                    url: url,
                    data: { id: $(this).find('input[name=id]').val(), returnUrl: location.toString() },
                    dataType: 'text',
                    success: function(url) {
                        window.location = url;
                    },
                    error: function(xhr) {
                        $('#login-dialog input[type=submit]').removeAttr('disabled').val('Log In');
                        alert(xhr.responseText || 'Could not login.');
                    }
                });

                return false;
            });

        $('#login-google a').click(function() {
            $('#login-dialog input[name=id]').val('https://www.google.com/accounts/o8/id');
            $('#login-dialog form').hide().submit();
            $('#login-google a').hide().parent().append('<span>Connecting to Google. Please wait...</span>');
            return false;
        });
    }
    $('#login-dialog').dialog('open').find('input[type=text]').focus();
}