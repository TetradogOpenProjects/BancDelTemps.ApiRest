<?php

namespace App;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\SoftDeletes;

class UserPermission extends Model
{
    use SoftDeletes;
    public function User(){
        return $this->belongsTo(User::class);
    }
    public function GrantedBy(){
        return $this->belongsTo(User::class,'grantedBy_id');
    }
    public function Permission(){
        return $this->belongsTo(Permission::class);
    }
}
