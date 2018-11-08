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

class ClassFactory{
   private $registeredClasses = array();
   static private $instance = NULL;

   private function __construct() {}

   static function getInstance(){
      if(self::$instance == NULL){
         self::$instance = new ClassFactory();
      }
      return self::$instance;
   }

   function registerClass($id, $creator_func){
      $this->registeredClasses[$id] = $creator_func;
   }

   function createObject($id, $args) {
     if(!isset($this->registeredClasses[$id])){
        return(NULL);
     }
     return($this->registeredClasses[$id]($args));
   }
}

function MyClassCreator(){
   return "creator";
}


$factory = ClassFactory::getInstance();

$factory->registerClass(1, "MyClassCreator");

$instance = $factory->createObject(1, array());
//mysql_query("at the end" . $instance);