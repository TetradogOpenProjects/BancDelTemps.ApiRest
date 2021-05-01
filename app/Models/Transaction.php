<?php

namespace App\Models;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\SoftDeletes;

class Transaction extends Model
{
    use HasFactory;
    use SoftDeletes;
    public function From(){
        return $this->belongsTo(User::class,'from_id');
    }
    public function To(){
        return $this->belongsTo(User::class,'to_id');
    }
}
