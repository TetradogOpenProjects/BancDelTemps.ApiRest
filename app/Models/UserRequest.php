<?php

namespace App\Models;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class UserRequest extends Model
{
    use HasFactory;
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
