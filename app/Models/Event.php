<?php

namespace App\Models;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Event extends Model
{
    use HasFactory;
    public function User(){
        return $this->belongsTo(User::class);
    }
    public function ApprovedBy(){
        return $this->belongsTo(User::class,'approvedBy_id');
    }
    public function Location(){
        return $this->belongsTo(Location::class);
    }
}
