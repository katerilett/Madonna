
var sDomDefault = "<'row'<'span6'l><'span6'f>r>t<'row'<'span6'i><'span6'p>>";
// Extend datatables to be able to sort formatted numbers.
function unformatNumber(a) {
	return (a == "-") ? 0 : a.replace(/[,$% ]/g, "");
}
jQuery.fn.dataTableExt.oSort['pretty-num-desc'] = function(a, b) {
	var x = unformatNumber(a);
	var y = unformatNumber(b);
	x = parseFloat(x);
	y = parseFloat(y);
	return ((x < y) ? 1 : ((x > y) ? -1 : 0));
};
jQuery.fn.dataTableExt.oSort['pretty-num-asc'] = function(a, b) {
	var x = unformatNumber(a);
	var y = unformatNumber(b);
	x = parseFloat(x);
	y = parseFloat(y);
	return ((x < y) ? -1 : ((x > y) ? 1 : 0));
};

// Detect formatted numbers automatically.
jQuery.fn.dataTableExt.aTypes.unshift(
	function(sData) {
		// First, get rid of HTML stuff!
		sData = sData.replace(/<.*>/, "");
		var sValidChars = "0123456789-,.$% ";
		var Char;
		var bDecimal = false;

		/* Check the numeric part */
		for (i = 0 ; i < sData.length ; i++) {
			Char = sData.charAt(i);
			if (sValidChars.indexOf(Char) == -1) {
				return null;
			}

			/* Only allowed one decimal place... */
			if (Char == ".") {
				if (bDecimal) {
					return null;
				}
				bDecimal = true;
			}
		}

		return 'pretty-num';
	}
);