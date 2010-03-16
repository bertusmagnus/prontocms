$.fn.reverse = [].reverse;
$.fn.editable = function(options) {
    function getUrl(id, global) {
        if (global) {
            return cms.urlBase + '_content/getglobalcontent/' + id;
        } else {
            return cms.urlBase + '_content/getcontent/' + id;
        }
    }

    function saveUrl(id, global) {
        if (global) {
            return cms.urlBase + '_content/saveglobalcontent/' + id;
        } else {
            return cms.urlBase + '_content/savecontent/' + id;
        }
    }

    function startEditing(editable) {
        var id = editable.attr('data-content-id');
        var global = editable.data('global');
        var data = global ? { x: new Date().getTime() } : { path: cms.page.path, x: new Date().getTime() };
        $.get(getUrl(id, global), data, function(content) {
            cms.dialogs.editContentDialog.open(content, saveUrl(id, global), global, editable);
        });
    }

    $(this)
        .data('global', options.global)
        .reverse()
        .each(function() {
            var action = $('<li class="cms-edit-page-content"><a href="#">Edit Page ' + ($(this).attr('data-content-id') || 'Text') + '</a></li>');
            action
                .data('target', $(this))
                .hover(
                    function() { $(this).data('target').addClass('cms-editable-hover'); },
                    function() { $(this).data('target').removeClass('cms-editable-hover'); }
                )
                .click(function() { startEditing($(this).data('target')); return false; });
            $('#cms-actions').prepend(action);
        });
};

$.fn.sortableMenu = function() {
    function xmlTree(tree) {
        var xml = ['<pages>'];
        function build(tree) {
            tree.find('> li').each(function() {
                var oldPath = $(this).attr('data-path');
                xml.push('<page path="', oldPath, '">');
                var ul = $(this).find('> ul');
                if (ul.length > 0) build(ul);
                xml.push('</page>');
            });
        }
        build(tree);
        xml.push('</pages>');
        return xml.join('');
    }
    function save(xml) {
        $.ajax({
            type: 'post',
            url: cms.urlBase + '_page/ReorganisePages?referrerPath=' + cms.page.path,
            data: xml,
            error: function(xhr) {
                alert('Error: ' + xhr.responseText);
            }
        });
    }
    this.sortable({
        forceHelperSize: true,
        forcePlaceholderSize: true,
        stop: function() {
            save(xmlTree($(this)));
        }
    });
};

function buildTemplateOptions(templates) {
    var options = [];
    for (var i = 0; i < templates.length; i++) {
        var text = templates[i].substr(0, templates[i].length - 4);
        options.push('<option value="', templates[i].toLowerCase(), '">', text, '</option>');
    }
    return options.join('');
}

function busy(message) {
    var d = $('<div title="Please Wait"><p class="busy">' + message + '</p></div>').dialog({
        modal: true,
        open: function() {
            $(this).parents(".ui-dialog:first").find(".ui-dialog-titlebar-close").remove();
        }
    });
    return function() { d.dialog('close'); };
}

//////////////////////////////////////////////////////

(function() {
    var initializing = false, fnTest = /xyz/.test(function() { xyz; }) ? /\b_super\b/ : /.*/;

    // The base Class implementation (does nothing)
    this.Class = function() { };

    // Create a new Class that inherits from this class
    Class.extend = function(prop) {
        var _super = this.prototype;

        // Instantiate a base class (but only create the instance,
        // don't run the init constructor)
        initializing = true;
        var prototype = new this();
        initializing = false;

        // Copy the properties over onto the new prototype
        for (var name in prop) {
            // Check if we're overwriting an existing function
            prototype[name] = typeof prop[name] == "function" &&
        typeof _super[name] == "function" && fnTest.test(prop[name]) ?
        (function(name, fn) {
            return function() {
                var tmp = this._super;

                // Add a new ._super() method that is the same method
                // but on the super-class
                this._super = _super[name];

                // The method only need to be bound temporarily, so we
                // remove it when we're done executing
                var ret = fn.apply(this, arguments);
                this._super = tmp;

                return ret;
            };
        })(name, prop[name]) :
        prop[name];
        }

        // The dummy class constructor
        function Class() {
            // All construction is actually done in the init method
            if (!initializing && this.init)
                this.init.apply(this, arguments);
        }

        // Populate our constructed prototype object
        Class.prototype = prototype;

        // Enforce the constructor to be what we expect
        Class.constructor = Class;

        // And make this class extendable
        Class.extend = arguments.callee;

        return Class;
    };
})();

//////////////////////////////////////////////////////

var AdminDialog = Class.extend({
    init: function(htmlFilename, b, options) {
        this.htmlFilename = htmlFilename;
        this.options = options;

        if (b) {
            if (typeof b === 'string') {
                var self = this;
                this.buttons = {};
                this.buttons[b] = function() { self.submit(); };
            } else {
                this.buttons = b;
            }
        } else {
            this.buttons = {};
        }
    },
    setupDialog: function($dialog) { },
    alterHtml: function(html) {
        return html.replace(/{{cms.urlBase}}/g, cms.urlBase)
                   .replace(/{{cms.page.path}}/g, cms.page.path);
    },
    downloadUI: function(autoOpen) {
        this.loading = true;
        var self = this;
        $.get(cms.urlBase + 'admin/dialogs/' + this.htmlFilename, null, function(html) {
            var dialogOptions = $.extend({ autoOpen: autoOpen, modal: true, buttons: self.buttons }, self.options);
            self.dialog = $(self.alterHtml(html)).dialog(dialogOptions);
            self.setupDialog(self.dialog);
            self.loading = false;
        });
    },
    open: function() {
        if (this.dialog) {
            this.dialog.dialog('open');
        } else {
            if (this.loading) return;
            this.downloadUI(true);
        }
    },
    close: function() {
        this.dialog.dialog('close');
    },
    clickHandler: function(handler) {
        var self = this;
        return function() {
            handler.call(self);
            return false;
        }
    },
    submit: function() {
        this.dialog.find('form').submit();
    }
});

//////////////////////////////////////////////////////

var NewPageDialog = AdminDialog.extend({
    init: function() {
        var self = this;
        this._super('NewPage.htm', 'Add Page');
    },
    setupDialog: function($dialog) {
        $dialog.find('input[name=referrerPath]').val(cms.page.path);
        $dialog.find('select[name=template]').append(buildTemplateOptions(cms.templates));
        $dialog.find('form').ajaxForm({
            beforeSend: function() {
                $dialog.find('form').hide();
                $dialog.append('<p class="busy">Adding page...</p>');
            },
            success: function(newPath) {
                location = cms.urlBase + newPath;
            },
            error: function(xhr) {
                $dialog.find('.busy').remove();
                $dialog.find('form').show();
                alert('Error adding page:\r\n' + xhr.responseText);
            }
        });
    }
});

//////////////////////////////////////////////////////

var EditPageDialog = AdminDialog.extend({
    init: function() {
        this._super('EditPage.htm', 'Save');
    },
    setupDialog: function($dialog) {
        $dialog.find('input[name=newTitle]').val(cms.page.title);
        $dialog.find('select[name=template]')
               .append(buildTemplateOptions(cms.templates))
               .val(cms.page.template);
        $dialog.find('input[name=navigation]').attr('checked', cms.page.navigation ? 'checked' : null);
        $dialog.find('input[name=name]').val(cms.page.name);
        $dialog.find('input[name=description]').val(cms.page.description);
        $dialog.find('.advanced-link').click(function() { $dialog.find('.advanced').toggle(); return false; });

        $dialog.find('form').ajaxForm({
            beforeSubmit: formBusy,
            success: reloadPage,
            error: formError
        });

        function formBusy() {
            $dialog.find('form').hide();
            $dialog.append('<p class="busy">Saving page...</p>');
        }

        function reloadPage() {
            var newName = $dialog.find('input[name=name]').val();
            if (newName != cms.page.name) {
                var temp = location.pathname.substr(1).split(/\//);
                temp.pop();
                location.replace(temp.join('/') + '/' + newName);
            } else {
                location.reload(true);
            }
        }

        function formError(xhr) {
            $dialog.find('.busy').remove();
            $dialog.find('form').show();
            alert('Error saving page:\r\n' + xhr.responseText);
        }
    }
});

//////////////////////////////////////////////////////

var OrganisePagesDialog = AdminDialog.extend({
    init: function() {
        this._super('OrganisePages.htm', 'Save', { position: 'top' });
    },
    buildTree: function() {
        var tree = $('#cms-navigation').clone().attr('id', 'cms-navigation-admin');
        tree.css('cursor', 'move');
        // only want page title text, not links or spans.
        // HACK: Safari doesn't like multiple selectors :( So have to use .add
        tree.find('a').add('#cms-navigation-admin span').each(function() { $(this).replaceWith(this.childNodes); });
        tree.find('li').prepend('<span class="ui-icon ui-icon-document" style="float:left;"/>');
        tree.find(':hidden').show();
        //tree.sortable({ axis: 'y' });
        tree.jTree();
        $('#jTreeHelper').css('margin-top', -parseInt($('body').css('margin-top')));
        return tree;
    },
    setupDialog: function OrganisePagesDialog_setupDialog($dialog) {
        $dialog.find('.tree-container').append(this.buildTree());
    },
    submit: function OrganisePagesDialog_submit() {
        this.close();
        var done = busy('Saving changes...');
        var tree = $('#cms-navigation-admin');
        function xmlTree() {
            var xml = ['<pages>'];
            function buildXml(lis) {
                lis.each(function() {
                    var oldPath = $(this).attr('data-path');
                    xml.push('<page path="', oldPath, '">');
                    buildXml($(this).find('> ul > li'));
                    xml.push('</page>');
                })
            }
            buildXml(tree.find('> li'));
            xml.push('</pages>');
            return xml.join('');
        }
        var self = this;
        $.ajax({
            type: 'post',
            url: cms.urlBase + '_page/ReorganisePages?referrerPath=' + cms.page.path,
            data: xmlTree(),
            success: function(newPath) {
                location = cms.urlBase + newPath;
            },
            error: function(xhr) {
                done();
                self.open();
                alert('Error: ' + xhr.responseText);
            }
        });
    }
});

//////////////////////////////////////////////////////

function deletePage() {
    if (!confirm('Delete this page?')) return;

    $('#cms-menu').dialog('close');
    var done = busy('Deleting Page');
    $.ajax({
        type: 'post',
        url: cms.urlBase + '_page/delete/' + cms.page.path,
        data: {},
        success: function() {
            window.location = cms.urlBase;
        },
        error: function() {
            done();
            alert('Error: Could not delete page.');
        }
    });
}

//////////////////////////////////////////////////////

var AdminUsersDialog = AdminDialog.extend({
    init: function() {
        var self = this;
        $.get(cms.urlBase + '_plugins/authorization/listadmins', null, function(admins) {
            function createAdminListItems() {
                var adminItems = [];
                for (var i = 0; i < admins.length; i++) {
                    adminItems.push('<li>', admins[i].Name);
                    if (admins[i].Id != cms.adminId) {
                        adminItems.push(' <a data-id="', admins[i].Id, '" href="#" class="delete">(delete)</a>');
                    }
                    adminItems.push('</li>');
                }
                return adminItems.join('');
            }
            self.adminListItems = createAdminListItems();
        }, 'json');

        this._super('AdminUsers.htm', { 'Add User': function() { self.addUser(); } });
    },
    addUser: function() {
        var self = this;
        $.post(cms.urlBase + '_plugins/authorization/createtoken', {}, function(token) {
            self.close();
            new NewTokenDialog(token).open();
        });
    },
    setupDialog: function($dialog) {
        $dialog.find('ul').append(this.adminListItems).find('.delete').click(this.deleteAdmin);
    },
    deleteAdmin: function() {
        var id = $(this).attr('data-id');
        var item = $(this).parent();
        if (confirm('Delete this admin user?')) {
            $.post(cms.urlBase + '_plugins/authorization/deleteadmin', { id: id }, function() {
                item.remove();
            });
        }
        return false;
    }
});

//////////////////////////////////////////////////////

var NewTokenDialog = AdminDialog.extend({
    init: function(token) {
        this.token = token;
        var self = this;
        this._super('NewToken.htm', { 'OK': function() { self.close() } });
    },
    setupDialog: function($dialog) {
        $dialog.find('input').val(this.token);
    }
});

//////////////////////////////////////////////////////

var EditContentDialog = AdminDialog.extend({
    init: function() {
        this._super('EditContent.htm', 'Save', { width: 800, height: 550, top: 0 });
        this.downloadUI(false);
    },
    setupDialog: function($dialog) {
        var fck = new FCKeditor('cms-content-textarea');
        fck.BasePath = cms.urlBase + "admin/fckeditor/";
        fck.ToolbarSet = 'Basic';
        fck.Height = 400;
        fck.ReplaceTextarea();
        this.fck = fck;
    },
    open: function(content, saveUrl, global, target) {
        this.content = content;
        this.saveUrl = saveUrl;
        this.isGlobalContent = global;
        this.target = target;
        this._super();
        $('#cms-content-textarea').val(this.content);
        if (FCKeditorAPI.GetInstance('cms-content-textarea'))
            FCKeditorAPI.GetInstance('cms-content-textarea').SetHTML(this.content);
    },
    submit: function() {
        var content = FCKeditorAPI.GetInstance('cms-content-textarea').GetXHTML();
        var data = this.isGlobalContent ? { content: content} : { path: cms.page.path, content: content };
        var self = this;
        $.ajax({
            type: 'post',
            url: this.saveUrl, data: data,
            success: function() {
                self.target.html(content);
                self.close();
            },
            error: function(xhr) {
                alert('Error Saving:\r\n' + xhr.responseText);
            }
        });
    }
});

//////////////////////////////////////////////////////

var ChangeSimplePassword = AdminDialog.extend({
    init: function() {
        this._super('ChangeSimplePassword.htm', 'Change Password', { width: 225 });
    },
    submit: function() {
        var self = this;
        this.dialog.find('form').ajaxSubmit({
            success: function() {
                self.close();
                self.dialog.find('form')[0].reset();
            },
            error: function(xhr) {
                alert(xhr.responseText);
            }
        });
    }
});

//////////////////////////////////////////////////////

$(function() {
    cms.dialogs = {
        newPageDialog: new NewPageDialog(),
        editPageDialog: new EditPageDialog(),
        organisePagesDialog: new OrganisePagesDialog(),
        editContentDialog: new EditContentDialog()
    };

    if (cms.authType == 'OpenId') {
        cms.dialogs.adminUsersDialog = new AdminUsersDialog();
    } else if (cms.authType == 'SimplePassword') {
        cms.dialogs.changeSimplePassword = new ChangeSimplePassword();
    }

    createToolbar();
    $('.editable').editable({ global: false });
    $('.global-editable').editable({ global: true });
    $('#cms-navigation').sortableMenu();

    function createToolbar() {
        $('body').append(
        '<div id="cms-toolbar">\
            <ul id="cms-actions">\
                <li class="cms-edit-page"><a href="#">Page Settings</a></li>\
                <li class="cms-delete-page"><a href="#">Delete This Page</a></li>\
                <li class="cms-new-page"><a href="#">Add A New Page</a></li>\
                <li class="cms-organise-pages"><a href="#">Organise Pages</a></li>\
            </ul>\
            <ul class="cms-auth-actions">\
                <li class="cms-admin-users"><a href="#">Manage Users</a></li>\
                <li class="cms-change-password"><a href="#">Change Password</a></li>\
                <li class="cms-logout"><a href="' + cms.urlBase + '_auth/logout">Log Out</a></li>\
            </ul>\
        </div>');
        var toolbar = $('#cms-toolbar');
        $('body').css('margin-top', toolbar.height() + 1);

        function showDialog(name) {
            return function() {
                cms.dialogs[name].open();
                return false;
            };
        }
        toolbar.find('.cms-new-page a').click(showDialog('newPageDialog'));
        toolbar.find('.cms-edit-page a').click(showDialog('editPageDialog'));
        toolbar.find('.cms-delete-page a').click(function() { deletePage(); return false; });
        toolbar.find('.cms-organise-pages a').click(showDialog('organisePagesDialog'));
        toolbar.find('.cms-admin-users a').click(showDialog('adminUsersDialog'));
        toolbar.find('.cms-change-password a').click(showDialog('changeSimplePassword'));
        if (cms.authType == 'SimplePassword') {
            toolbar.find('.cms-admin-users').hide();
        } else {
            toolbar.find('.cms-change-password').hide();
        }
    }
});