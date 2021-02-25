<?php

namespace App\Http\Controllers;

use Illuminate\Support\Facades\Auth;

use App\Request;
use App\Transaction;
use App\RequestTransaction;

class UserController extends Controller
{
    
    public function GetAll(){
        return Request::whereNotNull('approvedBy')
                      ->get()->toJson();   

    }
    public function GetNewData($lastUpdate){
        return Request::whereNotNull('approvedBy')
                      ->where('created_at','>',$lastUpdate)
                      ->get()->toJson();  
    }
    public function GetClearIds($lastUpdate){
        return RequestTransaction::where('created_at','>',$lastUpdate)
                          ->select('request_id')->distinct()
                          ->get()->toJson();  
    }
    public function GetTransactions(){
      
        return Transaction::where('From_id',Auth::user()->id)
                          ->orwhere('To_id',Auth::user()->id)
                          ->get()->toJson();
    }
}
