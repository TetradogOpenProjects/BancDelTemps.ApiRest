<?php

namespace App\Models;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\SoftDeletes;

class UserPermission extends Model
{
    use HasFactory;
    use SoftDeletes;
    protected $table="UserPermissions";
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
