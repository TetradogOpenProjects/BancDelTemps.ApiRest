<?php

namespace App;

use Illuminate\Database\Eloquent\Model;

class UserEvent extends Model
{
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
