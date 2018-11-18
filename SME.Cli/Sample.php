<?php 

function escape_input($input){
	return strtolower ($input);
}

$id = escape_input($_GET["id"]);
if ($id > 5){
	$query = "DELETE * FROM users WHERE id='" . $id . "'";

	for($i=0; $i<$id; $i++){
		$query = mysql_real_escape_string("DELETE * FROM users WHERE id='" . $id . "'");
		mysql_query($query);
	}
	
}