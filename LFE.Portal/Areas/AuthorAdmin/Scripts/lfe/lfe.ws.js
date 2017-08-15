//#region tabs manage
function setTabsState(mode) {
    if (mode == FormModes.edit) {
        $(window.STORE_TABS_SELECTOR + ' > li').removeClass('disabled');
    } else if (mode == FormModes.insert) {
        $.each($(window.STORE_TABS_SELECTOR + ' > li'), function (i) {
            if (i > 0) {
                $(this).addClass('disabled');
            }
        });
    }
}


function handleStoreSaveEvent(token) {
    var last = Number($(window.HID_STORE_SELECTOR).val());
    $(window.HID_STORE_SELECTOR).val(token.id);
    $('#li-page-name').html('Edit ' + token.name);
    setTabsState(FormModes.edit);

    if (last >= 0) return;
    //update forms url on first creation
    $.each($(window.STORE_TABS_SELECTOR + ' > li > form'), function () {
        var form = $(this);
        var url = form.attr('action');
        var newUrl = url.substring(0, url.lastIndexOf('/') + 1) + token.id;
        form.attr('action', newUrl);
    });
}
//#endregion

function onStoreSaved(response) {
    hideFormLoader();
    var kind = response.success ? NotificationKinds.Success : NotificationKinds.Error;
    var msg = response.success ? userMessages.WEB_STORE.STORE_SAVED : response.error;

    window.formUserNotifManager.show({ message: msg, kind: kind });

    if (response.success) {
        $(window.EDIT_FORM_SELECTOR).find('#StoreId').val(response.result.id);
        window.getNotifManagerInstance().notify(notifEvents.webstore.storeCreated, response.result);
        window.getNotifManagerInstance().notify(notifEvents.webstore.storeStateChanged, null);
    }
}

function updateEmbed(trackId, baseUrl) {
    var h = EMBED_TEMPLATE.replace('{{W}}', $('#d-embed-container').find('#Width').val())
                            .replace('{{H}}', $('#d-embed-container').find('#Height').val())
                            .replace('{{URL}}', $('#d-embed-container').find('#Url').val())
                            .replace('{{BASE}}', baseUrl)
                            .replace('{{TRACK}}', trackId);

    if (window.console) console.log($('#IsDisplayIframeBorder').prop('checked'));

    if ($('#IsDisplayIframeBorder').prop('checked')) {
        h = h.replace('{{BORDER_WIDTH}}', "1");
    }
    else {
        h = h.replace('{{BORDER_WIDTH}}', "0");
    }

    $('#EmbedCode').val(h);
}

var EMBED_TEMPLATE = '<iframe width="{{W}}" height="{{H}}" name="lfe_widget_iframe" id="lfe_widget_iframe" src="" frameBorder="{{BORDER_WIDTH}}" style="overflow:hidden" scrolling="no" ></iframe>' +
                     '<script>var lfeParentURL = "{{URL}}";</script>' +
                     '<script src="{{BASE}}Widget/{{TRACK}}/ParentScript" type="text/javascript"></script>';

//contents
var category2BeOpen = null;
var scroll2LastCatItem = false;
function expandCollapseCategories(t) {
    if (t == 1) showLoader();

    $.each($(window.CATEGORY_LIST_SELECTOR).find('.btn-collapse-first'), function () {
        var btn = $(this);
        var p = btn.parent();
        var exp = p.siblings('.first-contents');
        var isHidden = exp.is(":hidden");

        if (t == 1) {
            if (isHidden) btn.click();
        } else {
            if (!isHidden) btn.click();
        }
    });

    $(window.CATEGORY_LIST_SELECTOR).find('.btn-collapse-second').click();

    if (t == 1) hideLoader();

    setTimeout(function () {
        setReportScroll(CONTENT_CONTAINER_SELECTOR);
    }, 500);
}

function onCategoryListBound(e) {
    $.each(e.sender.wrapper.find('li .btn-collapse-first'), function () {
        var btn = $(this), $this = this;

        btn.unbind('click').click(function () {
            toggleCategory($this);
        });
    });

    $.each(e.sender.wrapper.find('li .ch-name'), function () {
        var btn = $(this), $this = $(this).siblings('.btn-collapse-first');

        btn.unbind('click').click(function () {
            toggleCategory($this);
        });
    });

    setReportScroll(CONTENT_CONTAINER_SELECTOR);

    setListSortable(window.CATEGORY_LIST_SELECTOR, window.saveCategoryOrderUrl, onListReordered, 'data-val');

    if (category2BeOpen == null) return;

    e.sender.wrapper.find('li[data-val=' + category2BeOpen + ']').find('.btn-collapse-first').click();
    e.sender.select(e.sender.wrapper.find('li[data-val=' + category2BeOpen + ']'));
    category2BeOpen = null;
    scroll2LastCatItem = true;

}


function toggleCategory($this) {
    var v = $($this).parent().siblings('.first-contents');
    var loaded = v.attr('data-load') == "true";

    var toBeOpen = v.is(":hidden");

    v.slideToggle("slow");

    if (toBeOpen) {
        $($this).addClass('expanded');

        if (!loaded) {
            var catId = $($this).parent().parent().attr('data-val');
            actionFormResultWithContainer(window.categoryContentsUrl, { id: catId }, '#cat-cn-' + catId, '#d-category-container');
            v.attr('data-load', "true");
        }
    } else {
        $($this).removeClass('expanded');
    }

    setTimeout(function () {
        setReportScroll(CONTENT_CONTAINER_SELECTOR);
    }, 500);

}

function addNewCategory() {
    expandCollapseCategories(2);
    var lv = $(window.CATEGORY_LIST_SELECTOR).data('kendoListView');
    var token = {
        WebStoreId: window.CURRENT_STORE_ID
        , WebStoreCategoryId: -1
        , CategoryName: 'new category'
        , IsPublic: true
        , Description: null
        , OrderIndex: lv.dataSource._data.length
    };

    lv.dataSource.add(token);

    setTimeout(function () {
        lv.edit(lv.element.children().last());
        window.getNotifManagerInstance().notify(notifEvents.webstore.storeStateChanged, null);
    }, 300);
}
function onStoreCategoryEdit(e) {
    e.item.find('input').focus();
    var btn = e.item.find('.k-update-button');
    btn.unbind('click').bind('click', function () {
        $('#frmEditWsCategory').submit();
    });
}

function onStoreCategoryListStateChanged(e) {
    //required to restore collapse functionality after cancel edit
    setTimeout(function () { onCategoryListBound(e); }, 300);
}

function onStoreCategoryRemoved(e) {
    if (!window.confirm('Delete category?')) {
        e.preventDefault();
    } else {
        window.getNotifManagerInstance().notify(notifEvents.webstore.storeStateChanged, null);
    }
}

function onStoreCategorySaved(response) {

    if (response.success) {
        window.getNotifManagerInstance().notify(notifEvents.webstore.storeStateChanged, null);
        response.message = 'Category saved';
        $(window.CATEGORY_LIST_SELECTOR).data('kendoListView').dataSource.read();
    }
    window.getNotifManagerInstance().notify(notifEvents.webstore.categorySaved, response);
    window.getNotifManagerInstance().notify(notifEvents.webstore.storeStateChanged, null);
}

function onCategoryContentsListBound(e) {
    var thisListSelector = '#' + e.sender.wrapper.attr('id');

    setListSortable(thisListSelector, window.saveItemOrderUrl, onListReordered, 'data-val');

    setTimeout(function () {
        setReportScroll(CONTENT_CONTAINER_SELECTOR);
        if (scroll2LastCatItem) {
            var last = $(thisListSelector).find('li').last();
            $(CONTENT_CONTAINER_SELECTOR).nanoScroller({ scrollTo: $(last) });

            scroll2LastCatItem = false;
        }
    }, 500);

    $.each(e.sender.wrapper.find('li .btn-remove'), function () {
        var btn = $(this);

        var id = btn.attr('data-val');

        btn.unbind('click').click(function () {
            if (window.confirm('Remove item?')) {
                ajaxAction(window.deleteItemUrl, { id: id }, onCategoryItemDeleted);
            }
        });
    });

    $.each(e.sender.wrapper.find('li .btn-collapse-second'), function () {
        var btn = $(this), $this = this;

        btn.unbind('click').click(function () {
            toggleCategoryItem($this);
        });
    });

    $.each(e.sender.wrapper.find('li .ch-name'), function () {
        var btn = $(this), $this = $(this).siblings('.btn-collapse-second');

        btn.unbind('click').click(function () {
            toggleCategoryItem($this);
        });
    });
}

function toggleCategoryItem($this) {
    var v = $($this).parent().siblings('.cn-details');

    var toBeOpen = v.is(":hidden");

    v.slideToggle("slow");

    if (toBeOpen) {
        $($this).addClass('expanded');
    } else {
        $($this).removeClass('expanded');
    }

    setTimeout(function () {
        setReportScroll(CONTENT_CONTAINER_SELECTOR);
    }, 500);

}

function onCategoryItemDeleted(response) {
    if (response.success) {
        window.getNotifManagerInstance().notify(notifEvents.webstore.storeStateChanged, null);
        response.message = 'Item deleted';
        $(window.CATEGORY_LIST_SELECTOR).find('li[data-val=' + response.result.id + ']').remove();
    }
    window.getNotifManagerInstance().notify(notifEvents.webstore.categorySaved, response);
    window.getNotifManagerInstance().notify(notifEvents.webstore.storeStateChanged, null);
}

function attachItems2Category() {
    var itemList = $(window.ITEM_LIST_SELECTOR).data('kendoListView');
    var items = itemList.select();
    if (items.length == 0) {

        window.formUserNotifManager.show({ message: 'select items', kind: NotificationKinds.Info });
        return;
    }

    var categoryList = $(window.CATEGORY_LIST_SELECTOR).data('kendoListView');
    var category = categoryList.select();

    if (category.length != 1) {
        window.formUserNotifManager.show({ message: 'select target category', kind: NotificationKinds.Info });
        return;
    }

    var catId = $(category[0]).attr('data-val');
    var itemsArray = [];

    $.each(items, function () {
        var item = itemList.dataSource.getByUid($(this).data('uid'));
        var itemId = item.ItemId, type = item.ItemType;
        var attached = $('#CategoryItems_' + catId + ' li[data-itemid=' + itemId + ']').length > 0;
        if (!attached) itemsArray.push({
            id: itemId,
            type: type
        });
    });

    if (itemsArray.length == 0) {
        window.formUserNotifManager.show({ message: 'all selected items already attached to category', kind: NotificationKinds.Info });
        return;
    }

    var data = { categoryId: catId, items: itemsArray };

    category2BeOpen = catId;

    window.ajaxAction(window.saveItemsUrl, { data: JSON.stringify(data) }, onItemsSaved);

}

function onItemsSaved(response) {
    if (response.success) {
        window.getNotifManagerInstance().notify(notifEvents.webstore.storeStateChanged, null);
        response.message = 'Items attached';
        $(window.ITEM_LIST_SELECTOR).data('kendoListView').clearSelection();
        $(window.CATEGORY_LIST_SELECTOR).data('kendoListView').dataSource.read();
    } else {
        category2BeOpen = null;
    }
    window.getNotifManagerInstance().notify(notifEvents.webstore.categorySaved, response);
    window.getNotifManagerInstance().notify(notifEvents.webstore.storeStateChanged, null);
}

// store items
var storeItemLoaded = false;
function onStoreItemsListBound(e) {

    $.each(e.sender.wrapper.find('li .btn-collapse-second'), function () {
        var btn = $(this), $this = this;

        btn.unbind('click').click(function () {
            toggleItem($this);
        });
    });
    //$.each(e.sender.wrapper.find('li .ch-name'), function () {
    //    var btn = $(this), $this = $(this).siblings('.btn-collapse-second');

    //    btn.unbind('click').click(function () {
    //        toggleItem($this);
    //    });
    //});
    window.setReportScroll(window.STORE_ITEMS_CONTAINER_SELECTOR);

    if (storeItemLoaded === true) return;

    storeItemLoaded = true;

    $('input[name="FilterMethod"][value="MY"]').prop('checked', true);

    onFilterOptionChanged($('input[name="FilterMethod"][value="MY"]'));
}
function toggleItem($this) {
    var v = $($this).parent().siblings('.cn-details');

    var toBeOpen = v.is(":hidden");

    v.slideToggle("slow");

    if (toBeOpen) {
        $($this).addClass('expanded');
    } else {
        $($this).removeClass('expanded');
    }

    setTimeout(function () {
        window.setReportScroll(window.STORE_ITEMS_CONTAINER_SELECTOR);
    }, 500);

}

//#region filters
function onItemsRequested() {    
    var data = {
        name: $('#searchItemName').val(),
        authorId: $('#searchAuthorId').val()
    };
    
    return data;
}
function onFilterOptionChanged($this) {
    var f = $($this).attr('value');
    var combo = $('#ddlAuthors').data('kendoComboBox');
    var au = $('#auItems').data('kendoAutoComplete');
    $('#searchItemName').val(null);
    $('#searchAuthorId').val(null);
    au.enable(false);
    au.value(null);
    combo.enable(false);
    combo.value(null);

    var filter = {
        filters: []
    };

    switch (f) {
        case "ALL":
            break;
        case "MY":
//            var f1 = { field: "AuthorId", operator: "eq", value: window.currentUserId };
//            filter.filters.push(f1);
            $('#searchAuthorId').val(window.currentUserId);
            rebindAvailableItems();
            return;
        case "AUTHOR":
            combo.enable(true);
            break;
        case "SEARCH":
            au.enable(true);
            break;
    }

   // filterStoreItems(filter);
}

function filterStoreItems(filter) {
    var listId = window.ITEM_LIST_SELECTOR.substring(1);
    var filterExists = filter.filters.length > 0;

    if (filterExists) {
        window.checkFilter(filter, listId, "kendoListView");
        window.filterReport(filter, listId, "kendoListView");
    } else {
        window.filterReport(null, listId, "kendoListView");
    }
}
function onAdditionalData() {
    return {
        name: $("#auItems").val()
    };
}
function onAuthorSelected(e) {
    var dataItem = this.dataItem(e.item.index());
    var id = dataItem.id;
    $('#searchAuthorId').val(id);
//    var filter = { filters: [] };
//    var f1 = { field: "AuthorId", operator: "eq", value: authorId };
//    filter.filters.push(f1);
    // window.filterStoreItems(filter);

    setTimeout(rebindAvailableItems, 100);

}

function onSerachItemsChanged() {
    var value = this.value();
    //searchStoreItems(value);
}

function onSerachItemsSelected(e) {
    var value = e.item.text();
    $('#searchItemName').val(value);
    setTimeout(rebindAvailableItems, 100);
    //searchStoreItems(value);
}

function rebindAvailableItems() {
    $('#StoreItems').data("kendoListView").dataSource.read();
}

function searchStoreItems(text) {
    //
    var filter = { logic: "or", filters: [] };
    var f = { field: "ItemName", operator: "contains", value: text };
    filter.filters.push(f);
    f = { field: "Description", operator: "contains", value: text };
    filter.filters.push(f);
    f = { field: "AuthorName", operator: "contains", value: text };
    filter.filters.push(f);
    filterStoreItems(filter);
}
//#endregion

function bindItemSort() {
    $.each($('.cn-sort a'), function () {
        var btn = $(this);

        btn.unbind('click').click(function (e) {
            e.preventDefault();
            var state = $(this).attr('data-state');
            resetSortState();
            var dir, icon = $(this).find('i'), target = $(this).attr('data-target'), field;
            //set direction
            switch (state) {
                case 'none':
                    dir = 'asc';
                    icon.addClass('k-i-arrow-n').css('visibility', 'visible');
                    break;
                case 'asc':
                    dir = 'desc';
                    icon.addClass('k-i-arrow-s').css('visibility', 'visible');
                    break;
                case 'desc':
                    dir = 'none';
                    icon.css('visibility', 'hidden');
                    break;
                default:
                    return;
            }

            var ds = $(window.ITEM_LIST_SELECTOR).data('kendoListView').dataSource;

            if (dir == 'none') {
                ds.sort({});
                return;
            }

            //set field
            switch (target) {
                case 'Author':
                    field = 'AuthorName';
                    break;
                case 'ItemName':
                    field = 'ItemName';
                    break;
                case 'Price':
                    field = 'Price';
                    break;
                case 'SubscriptionPrice':
                    field = 'SubscriptionPrice';
                    break;
                case 'Commission':
                    field = 'AffiliateCommission';
                    break;
                default:
                    return;
            }
            $(this).attr('data-state', dir);

            ds.sort({ field: field, dir: dir });
        });
    });
}

function resetSortState() {
    $('.cn-sort a').attr('data-state', 'none');
    $('.cn-sort a i').removeClass('k-i-arrow-n k-i-arrow-s').css('visibility', 'hidden');
}