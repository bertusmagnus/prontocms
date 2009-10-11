$(function() {
    var form = $('#contact-form');
    var name = form.find('input[name=name]');
    var emailAddress = form.find('input[name=emailAddress]');
    var message = form.find('textarea[name=message]');

    function sendSuccess() {
        form.hide();
        $('#contact-form-send-success').show();
    }

    form.submit(function() {
        var url = form[0].action;
        $.ajax({
            type: 'post',
            url: url,
            data: { name: name.val(), emailAddress: emailAddress.val(), message: message.val() },
            success: sendSuccess,
            error: function() { alert('Could not send message. Make sure your email address is correct.'); }
        });
        return false; // prevent default browser submit
    });
});