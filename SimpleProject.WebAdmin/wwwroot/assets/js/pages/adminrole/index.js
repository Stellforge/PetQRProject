function editRecord() {
    $app.gotoUrl("/adminroles/edit/" + $(this).attr("data-id"));
}

function deleteRecord() {
    var id = $(this).attr("data-id");
    $app.confirm($lang.deleteConfirm, function () {
        $app.callJx("/adminroles/delete", "body", { id: id }, function (result) {
            $grid(".ruleway-grid").load();
        });
    });
}

function showUpload() {
    $app.showUpload('/adminroles/upload', function () {
        $grid(".ruleway-grid").load();
    });
}