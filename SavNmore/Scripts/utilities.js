function validateSearch() {if (document.getElementById("search").value == '') return false;return true;}
function confirmDelete() { var answer = confirm('Delete all checked?'); return answer; }
function confirmRemove() { var answer = confirm('Remove all members checked?'); return answer; }
function confirmAdd() { var answer = confirm('Add all members checked?'); return answer; }
function confirmAddAll() { var answer = confirm('Add all users to this role?'); return answer; }
function confirmRemoveAll() { var answer = confirm('Remove all users, but Admin, from this role?'); return answer; }
function confirmIt() { var answer = confirm('You sure?'); return answer; }
 
function checkUncheckAll() {
    $("input[name = deleteInputs]").each(function () {
        if ($('input[name=checkuncheckall]')[0].checked == 1) { this.setAttribute('checked', true); }
        else { this.checked = false; }
    });
}
function validatePageNum() {
    var x = $("#pagenum").val();
    
    if (!isNaN(parseFloat(x)) && isFinite(x)) {
        return;
    } else {
        $("#pagenum").val('1');
    }
}
 function AddItem(itemid,storeid) {
        var data = {
            storeId: storeid, 
            itemId: itemid
        };
        ItemAdded(itemid);
        //$.post('/Items/AddToList',data, ItemAdded);
        $.post('/Items/AddToList', data);
    }
    function AddItemShowTotal(itemid, storeid) {
        $.ajax({
            type: "POST",
            url: "/Items/Add1ItemUpdateTotal",
            dataType: 'text',
            data: { storeId: storeid, itemId: itemid },
            success: function (total) { var tl = total.replace(/"/g, ""); ItemAdded(itemid);
                toast('Added item.<br/>New store total: <b>' + tl + '</b><br/><a href=\'../../ShoppingList/Index\'>Go to List</a>');
            },
            error: function () { }
        });
    }
    function RemoveItemShowTotal(itemid, storeid) {
        $.ajax({
            type: "POST",
            url: "/Items/RemoveFromList",
            dataType: 'text',
            data: { storeId: storeid, itemId: itemid },
            success: function (total) {
                ItemRemoved(itemid);
                var tl = total.replace(/"/g, "");
                toast('Removed item.<br/>New store total: <b>' + tl + '</b><br/><a href=\'../../ShoppingList/Index\'>Go to List</a>');
            },
            error: function () { }
        });
    }
    function RemoveItem(itemid,storeid) {
        var data = {
            storeId: storeid, 
            itemId: itemid
        };
        ItemRemoved(itemid);
        //$.post('/Items/RemoveFromList',data, ItemRemoved);
        $.post('/Items/RemoveFromList', data);
    }
    function ItemAdded(itemid) {
        //get the remove button and show it
         
        $('#r' + itemid).show();
    }
    function ItemRemoved(itemid) {
        //get the remove button and hide it
      
        $('#r' + itemid).hide();
    }
    function toast(sMessage) {
        var container = $(document.createElement("div"));
        container.addClass("toast");

        var message = $(document.createElement("div"));
        message.addClass("message");
        message.html(sMessage);
        message.appendTo(container);

        container.appendTo(document.body);

        container.delay(100).fadeIn("slow", function () {
            $(this).delay(1000).fadeOut("slow", function () {
                $(this).remove();
            });
        });
    }
    function EmailList() {
        var ee = $('#email').val();
        $.ajax({
            type: "POST",
            url: "/ShoppingList/EmailList",
            dataType: 'text',
            data: { to: ee },
            success: function (msg) { var tl = msg.replace(/"/g, ""); toast(tl); $('#email').val(''); },
            error: function () { toast('Unable to email.'); $('#email').val(''); }
        });
    }
    function deleteItem(rowId) {
        $('#t' + rowId).remove();
    }
    function RemoveItemFromList(itemid, storeid) {
        $.ajax({
            type: "POST",
            url: "/Items/RemoveFromList",
            dataType: 'text',
            data: { storeId: storeid, itemId: itemid },
            success: function (total) { deleteItem(itemid); var tl = total.replace(/"/g, ""); $('#ttl' + storeid).html(tl); },
            error: function () { }
        });
    }
    function Remove1ItemUpdateTotal(itemid, storeid) {
        $.ajax({
            type: "POST",
            url: "/Items/Remove1ItemUpdateTotal",
            dataType: 'text',
            data: { storeId: storeid, itemId: itemid },
            success: function (total) { var tl = total.replace(/"/g, ""); $('#ttl' + storeid).html(tl); },
            error: function () { }
        });
    }
    function Add1ItemToList(itemid, storeid) {
        var data = {
            storeId: storeid,
            itemId: itemid
        };
        $.post('/Items/AddToList', data);
    }
    function Add1ItemUpdateTotal(itemid, storeid) {
        $.ajax({
            type: "POST",
            url: "/Items/Add1ItemUpdateTotal",
            dataType: 'text',
            data: { storeId: storeid, itemId: itemid },
            success: function (total) { var tl = total.replace(/"/g, ""); $('#ttl' + storeid).html(tl); },
            error: function () { }
        });
    }

    function Add1Item(itemId, storeId) {
        try {
            var currentQty = parseInt($('#q' + itemId).html());
            //add the item
            currentQty++;            
            Add1ItemUpdateTotal(itemId, storeId);
            $('#q' + itemId).html(currentQty);
        } catch (e) {

        }
    }
    function Remove1Item(itemId, storeId) {
        try {
            var currentQty = parseInt($('#q' + itemId).html());
            //add the item
            if (currentQty > 0) {
                currentQty--;
                Remove1ItemUpdateTotal(itemId, storeId);
                //deleteItem(itemId);
                $('#q' + itemId).html(currentQty);
            }

        } catch (e) {

        }
    }
   
 
