<?php

namespace Database\Factories;

use App\Models\File;
use App\Models\User;

use Illuminate\Database\Eloquent\Factories\Factory;

class FileFactory extends Factory
{
    /**
     * The name of the factory's corresponding model.
     *
     * @var string
     */
    protected $model = File::class;

    /**
     * Define the model's default state.
     *
     * @return array
     */
    public function definition()
    {
        return [
            'approvedBy_id'=>User::factory(),
            'name'=>$this->faker->name(),
            'format'=>'bin',
            'canDelete'=>$this->faker->boolean()
        ];
    }
}
