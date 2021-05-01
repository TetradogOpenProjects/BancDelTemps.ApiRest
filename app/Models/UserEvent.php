<?php

namespace App\Models;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class UserEvent extends Model
{
    use HasFactory;
    public function Event(){
        return $this->belongsTo(Event::class);
    }
    public function ApprovedBy(){
        return $this->belongsTo(User::class,'approvedBy_id');
    }
    public function User(){
        return $this->belongsTo(User::class);
    }
}
