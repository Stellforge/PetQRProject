function editRecord() {
    $app.gotoUrl("/customfields/edit/" + $(this).attr("data-id"));
}

function deleteRecord() {
    var id = $(this).attr("data-id");
    $app.confirm($lang.deleteConfirm, function () {
        $app.callJx("/customfields/delete", "body", { id: id }, function (result) {
            $grid(".ruleway-grid").load();
        });
    });
}

function showUpload() {
    $app.showUpload('/customfields/upload', function () {
        $grid(".ruleway-grid").load();
    });
}