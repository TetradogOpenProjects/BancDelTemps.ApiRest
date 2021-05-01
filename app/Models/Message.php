<?php

namespace App\Models;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\SoftDeletes;

class Message extends Model
{
    use HasFactory;
    use SoftDeletes;
    public function From(){
        return $this->belongsTo(User::class,'From_id');
    }
    public function To(){
        return $this->belongsTo(User::class,'To_id');
    }


}
