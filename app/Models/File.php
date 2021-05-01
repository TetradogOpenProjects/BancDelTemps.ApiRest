<?php

namespace App;

use Illuminate\Database\Eloquent\Model;

class File extends Model
{
    public function ApprovedBy(){
        return $this->belongsTo(User::class,'approvedBy_id');
    }
}
