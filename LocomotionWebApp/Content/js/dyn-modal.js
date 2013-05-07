
function popupModalFullHtml(titleHTML, contentHTML, buttonsHTML) {
	$("#jsModalLabel").html(titleHTML);
	$("#jsModal .modal-body").html(contentHTML);
	$("#jsModal .modal-footer").html(buttonsHTML);

	$("#jsModalForm").removeAttr("action");

	$("#jsModal").modal();
}

function popupModalHtml(titleHTML, contentHTML) {
	popupModalFullHtml(titleHTML, contentHTML, 
		'<button class="btn btn-primary" data-dismiss="modal" aria-hidden="true">Close</button>'
	);
}

function popupModalUrl(titleHTML, contentURL) {
	popupModalHtml(
		titleHTML,
		'<div style="text-align: center;"><img src="/Content/img/ajax-loader.gif" /></div>'
	);
	$("#jsModal .modal-body").load(contentURL);
}



