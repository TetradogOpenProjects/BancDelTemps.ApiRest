<?php

namespace App;

use Illuminate\Database\Eloquent\Model;

class Event extends Model
{
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
