$(function() {

    $('#cms-admin-link').click(function() {
        showLogin();
        return false;
    });

    function loginClick() {
        $.ajax({
            type: 'post',
            url: cms.urlBase + '_auth/login',
            data: { password: $(this).find('input').val() },
            error: function(xhr) {
                alert(xhr.responseText || 'Sorry, could not log in!');
            },
            success: function() {
                location.reload(true);
            }
        });
    }

    function showLogin() {
        $('body').css('overflow', 'hidden');
        $('<div title="Enter Password">\
             <form method="post" action="">\
                <input type="password" name="password" style="padding: 4px; width: 254px" />\
             </form\
           </div>')
        .dialog({
            modal: true,
            buttons: { 'Log In': loginClick },
            close: function() { $('body').css('overflow', 'auto'); }
        })
        .find('form').submit(function() { loginClick.call(this); return false; }); ;
    }

});

