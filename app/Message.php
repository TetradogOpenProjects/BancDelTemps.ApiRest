<?php

namespace App;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\SoftDeletes;

class Message extends Model
{
    use SoftDeletes;
    public function From(){
        return $this->belongsTo(User::class,'From_id');
    }
    public function To(){
        return $this->belongsTo(User::class,'To_id');
    }


}
