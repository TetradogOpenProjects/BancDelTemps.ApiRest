<?php

namespace App\Http\Controllers;

use Illuminate\Foundation\Auth\Access\AuthorizesRequests;
use Illuminate\Foundation\Bus\DispatchesJobs;
use Illuminate\Foundation\Validation\ValidatesRequests;
use Illuminate\Routing\Controller as BaseController;

use Illuminate\Support\Facades\Auth;

use App\Request;
use App\Transaction;

class Controller extends BaseController
{
    use AuthorizesRequests, DispatchesJobs, ValidatesRequests;

    public function GetAll(){
        return Request::whereNotNull('approvedBy')->get()->toJson();   

    }
    public function GetNewData($lastUpdate){
        return Request::whereNotNull('approvedBy')
                      ->where('created','>',$lastUpdate)
                      ->get()->toJson();  
    }
    public function GetClearIds($lastUpdate){
        return Transaction::where('created','>',$lastUpdate)
                          ->select('request_id')->distinct()
                          ->get()->toJson();  
    }
    public function GetTransactions(){
      
        return Transaction::where('To_id',Auth::user()->id)->get()->toJson();
    }
    
}
