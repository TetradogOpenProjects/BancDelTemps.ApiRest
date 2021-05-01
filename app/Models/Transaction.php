<?php

namespace App;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\SoftDeletes;

class Transaction extends Model
{
    use SoftDeletes;
    public function From(){
        return $this->belongsTo(User::class,'from_id');
    }
    public function To(){
        return $this->belongsTo(User::class,'to_id');
    }
}
