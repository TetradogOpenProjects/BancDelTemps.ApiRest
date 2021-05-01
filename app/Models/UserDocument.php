<?php

namespace App;

use Illuminate\Database\Eloquent\Model;

class UserDocument extends Model
{
    public function File(){
        return $this->belongsTo(File::class);
    }
}
