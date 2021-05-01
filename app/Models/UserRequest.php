<?php

namespace App;

use Illuminate\Database\Eloquent\Model;

class UserRequest extends Model
{
    public function Request(){
        return $this->belongsTo(Request::class);
    }
    public function ApprovedBy(){
        return $this->belongsTo(User::class,'approvedBy_id');
    }
    public function User(){
        return $this->belongsTo(User::class);
    }
}
