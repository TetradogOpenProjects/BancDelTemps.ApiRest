<?php

namespace App\Http\Controllers;

use Illuminate\Foundation\Auth\Access\AuthorizesRequests;
use Illuminate\Foundation\Bus\DispatchesJobs;
use Illuminate\Foundation\Validation\ValidatesRequests;
use Illuminate\Routing\Controller as BaseController;

use App\Request;
use App\Cleared;

class Controller extends BaseController
{
    use AuthorizesRequests, DispatchesJobs, ValidatesRequests;

    public function GetAll(){
        return Request::all()->toJson();   

    }
    public function GetNewData($lastUpdate){
        return Request::where('InitDate','>',$lastUpdate)
                       ->get()->toJson();  
    }
    public function GetClearIds($lastUpdate){
        return Request::where('ClearedDate','>',$lastUpdate)
                       ->pluck('id')->get()->toJson();  
    }
    
}
