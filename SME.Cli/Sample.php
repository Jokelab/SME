<?php 

function sanitize($input){
	return strtolower ($input);
}


$id = sanitize($_GET["id"]);
if ($id > 5){
	$query = "DELETE * FROM users WHERE id='" . $id . "'";
	mysql_query($query);
	$query = mysql_real_escape_string("DELETE * FROM users WHERE id='" . $id . "'");
	mysql_query($query);
	
}
else{
	mysql_query("SELECT * FROM users WHERE id='" . $id . "'");
}