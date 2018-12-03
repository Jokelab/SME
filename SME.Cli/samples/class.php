<?php

// base class with member properties and methods
class Vegetable {
 
   var $color;
   var $name;

   function __construct($name, $color="green")
   {
	   $this->name = $name;
       $this->color = $color;
   }

	function get_name(){
		return $this->name;
	}


   function what_color()
   {
       return $this->color;
   }
   
   function show(){
	   return $this->get_name() . " (" . $this->what_color() . ")";
   }

} 

// end of class Vegetable
$name = $_GET["name"];
$veg = new Vegetable($name, "orange");
echo $veg->show();
