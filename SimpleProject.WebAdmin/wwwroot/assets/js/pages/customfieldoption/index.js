function editRecord() {
	$app.gotoUrl("/customfieldoptions/edit/" + $(this).attr("data-id"));
}

function deleteRecord() {
	var id = $(this).attr("data-id");
	$app.confirm($lang.deleteConfirm, function () {
		$app.callJx("/customfieldoptions/delete", "body", { id: id }, function (result) {
			$grid(".ruleway-grid").load();
		});
	});
}

function showUpload() {
	$app.showUpload('/customfieldoptions/upload', function () {
		$grid(".ruleway-grid").load();
	});
}